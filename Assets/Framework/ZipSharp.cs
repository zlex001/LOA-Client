using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Framework
{
    public class ZipSharp
    {
        private byte[] zipData = null;
        private string destPath = null;
        /// <summary>
        /// Is complete, progress percentage, error message
        /// </summary>
        private Action<bool, float, string> statusAction;
        /// <summary>
        /// Compress folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="destPath"></param>
        public void CompressFolder(string folderPath, string destPath)
        {
            FileStream fsOut = System.IO.File.Create(destPath);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

            zipStream.Password = null;  // optional. Null is the same as not setting. Required if using AES.

            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign folderOffset = 0.
            int folderOffset = folderPath.Length + (folderPath.EndsWith("\\") ? 0 : 1);

            CompressFolder(folderPath, zipStream, folderOffset);

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        /// <summary>
        /// Unzip from file
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="destPath"></param>
        /// <param name="statusAction"></param>
        public void UnZipByFile(string zipPath, string destPath, Action<bool, float, string> statusAction)
        {
            if (System.IO.File.Exists(zipPath))
            {
                UnZipByByte(System.IO.File.ReadAllBytes(zipPath), destPath, statusAction);
            }
            else
            {
                Debug.LogError("File does not exist: " + zipPath);
            }
        }

        /// <summary>
        /// Unzip from byte array
        /// </summary>
        /// <param name="zipData"></param>
        /// <param name="destPath"></param>
        /// <param name="statusAction"></param>
        public void UnZipByByte(byte[] zipData, string destPath, Action<bool, float, string> statusAction)
        {
            if (zipData != null)
            {
                this.statusAction = statusAction;
                this.zipData = zipData;
                this.destPath = destPath;

                // Use Loom to run in thread
                Loom.RunAsync(
                    () =>
                    {
                        Thread thread = new Thread(ZipFunc);
                        thread.Start();
                    }
                    );
            }
            else
            {
                Loom.QueueOnMainThread(() =>
                {
                    statusAction(false, 0, "zipData  null");
                });
                Debug.LogError("zipData is null");
            }
        }

        /// <summary>
        /// Unzip function
        /// </summary>
        public void ZipFunc()
        {
            ZipFile zf = null;
            try
            {
                //FileStream fs = File.OpenRead(zipFilePath);
                MemoryStream memoryStream = new MemoryStream(zipData);
                zf = new ZipFile(memoryStream);

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    string entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    string fullZipToPath = System.IO.Path.Combine(destPath, entryFileName);
                    string directoryName = System.IO.Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        System.IO.Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = System.IO.File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                        //Debug.LogError(zipEntry.ZipFileIndex + "   " + zf.Count);
                        int zipCount = (int)zipEntry.ZipFileIndex + 1;
                        int maxCount = (int)zf.Count;

                        float percent = (float)zipCount / maxCount;
                        if (zipCount >= zf.Count)
                        {
                            Loom.QueueOnMainThread(() =>
                            {
                                statusAction(true, percent, "");
                            });
                        }
                        else
                        {
                            Loom.QueueOnMainThread(() =>
                            {
                                statusAction(false, percent, "");
                            });
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Loom.QueueOnMainThread(() =>
                {
                    statusAction(false, 0, ex.ToString());
                });
            }
            zf.IsStreamOwner = true; // Makes close also shut the underlying stream
            zf.Close(); // Ensure we release resources
        }


        ////---------------------------------Compress---------------------------------

        /// <summary>
        /// Compress folder recursively
        /// </summary>
        /// <param name="path"></param>
        /// <param name="zipStream"></param>
        /// <param name="folderOffset"></param>
        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = System.IO.Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = System.IO.File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = System.IO.Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
    }
}
