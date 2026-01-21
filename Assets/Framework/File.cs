using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework
{

    public static class File
    {
        public static string[] GetFiles(string path, params string[] ignoredExtensions)
        {
            List<string> result = new List<string>();
            CollectFiles(path, result);

            if (ignoredExtensions.Length == 0)
                return result.ToArray();

            return result.FindAll(f =>
            {
                foreach (var ext in ignoredExtensions)
                    if (f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                        return false;
                return true;
            }).ToArray();
        }

        private static void CollectFiles(string path, List<string> list)
        {
            foreach (var file in Directory.GetFiles(path))
                list.Add(file.Replace("\\", "/"));

            foreach (var dir in Directory.GetDirectories(path))
                CollectFiles(dir, list);
        }

        public static string GetFileMd5(string file)
        {
            try
            {
                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                byte[] hash = MD5.Create().ComputeHash(fs);
                StringBuilder sb = new();
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[File] MD5 calculation failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static void SaveText(string path, string content) => System.IO.File.WriteAllText(path, content);

        public static string GetText(string path) =>
            System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : string.Empty;

        public static void LoadFile(MonoBehaviour host, string filename, Action<bool, byte[]> callback)
        {
            host.StartCoroutine(LoadFileCoroutine(filename, callback));
        }

        private static IEnumerator LoadFileCoroutine(string filename, Action<bool, byte[]> callback)
        {
            string path = FrameworkPath.Package + "/" + filename;
            using UnityWebRequest req = UnityWebRequest.Get(path);
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                callback(true, req.downloadHandler.data);
            else
                callback(false, null);
        }
    }
}
