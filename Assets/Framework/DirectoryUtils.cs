using System;
using System.IO;
using UnityEngine;
namespace Framework
{
    public static class DirectoryUtils
    {
        public static bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting = true)
        {
            bool ret = false;
            try
            {
                SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
                DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

                if (System.IO.Directory.Exists(SourcePath))
                {
                    if (System.IO.Directory.Exists(DestinationPath) == false)
                        System.IO.Directory.CreateDirectory(DestinationPath);

                    foreach (string fls in System.IO.Directory.GetFiles(SourcePath))
                    {
                        FileInfo flinfo = new FileInfo(fls);
                        flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
                    }
                    foreach (string drs in System.IO.Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo drinfo = new DirectoryInfo(drs);
                        if (CopyDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting) == false)
                            ret = false;
                    }
                }
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }
}
