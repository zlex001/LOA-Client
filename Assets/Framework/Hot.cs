using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public class Hot : Singleton<Hot>
    {
        private string Platform =>
#if UNITY_ANDROID
            "android";
#elif UNITY_IOS
            "ios";
#else
            "windows";
#endif

        private string Url { get; set; }
        private string LocalVersion { get; set; }

        public string Version => PlayerPrefs.GetString("HOT_VERSION");
        public Action<string> OnStatusChanged;

        private void Notify(string msg) => OnStatusChanged?.Invoke(msg);

        public class Res
        {
            public string name;
            public string md5;
            public int size;

            public Res(string line)
            {
                var parts = line.Split('|');
                name = parts[0];
                md5 = parts[1];
                size = int.Parse(parts[2]);
            }
        }

        public void Init(string url, string localVersion)
        {
            Url = url;
            LocalVersion = localVersion;
            Http.Instance.AcceptLanguage = GetLanguage();
        }

        private string GetLanguage()
        {
            return PlayerPrefs.GetString("LANGUAGE", "ChineseSimplified");
        }

        public void Begin(Action<bool, string, Dictionary<string, string>> callback)
        {
            string versionUrl = $"{Url}/api/hot/version";
            Http.Instance.RequestGet(versionUrl, (success, content) =>
            {
                if (!success)
                {
                    callback(false, "network_error", null);
                    return;
                }

                try
                {
                    var data = JsonUtility.FromJson<VersionResponse>(content);
                    
                    Dictionary<string, string> texts = null;
                    if (data.texts != null)
                    {
                        texts = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(data.texts.checking)) 
                            texts["checking_version"] = data.texts.checking;
                        if (!string.IsNullOrEmpty(data.texts.check_failed)) 
                            texts["version_check_failed"] = data.texts.check_failed;
                    }

                    PlayerPrefs.SetString("HOT_VERSION", data.version);
                    PlayerPrefs.Save();
                    callback(LocalVersion != data.version, null, texts);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Hot] Failed to parse version response: {ex.Message}");
                    callback(false, "parse_error", null);
                }
            });
        }

        [System.Serializable]
        private class VersionResponse
        {
            public string version;
            public VersionTexts texts;
        }

        [System.Serializable]
        private class VersionTexts
        {
            public string checking;
            public string check_failed;
        }

        public void Check(Action<bool, List<Res>, Dictionary<string, string>> callback)
        {
            string checkUrl = $"{Url}/api/hot/check";
            Http.Instance.RequestGet(checkUrl, (success, content) =>
            {
                if (!success)
                {
                    Debug.LogError($"[Hot] Resource check request failed");
                    callback(false, null, null);
                    return;
                }

                try
                {
                    var data = JsonUtility.FromJson<CheckResponse>(content);
                    
                    Dictionary<string, string> texts = null;
                    if (data.texts != null)
                    {
                        texts = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(data.texts.comparing)) 
                            texts["comparing_resources"] = data.texts.comparing;
                        if (!string.IsNullOrEmpty(data.texts.resources_updated)) 
                            texts["resources_updated"] = data.texts.resources_updated;
                        if (!string.IsNullOrEmpty(data.texts.start_downloading)) 
                            texts["start_downloading"] = data.texts.start_downloading;
                        if (!string.IsNullOrEmpty(data.texts.download_complete)) 
                            texts["download_complete"] = data.texts.download_complete;
                        if (!string.IsNullOrEmpty(data.texts.download_failed)) 
                            texts["download_failed"] = data.texts.download_failed;
                        if (!string.IsNullOrEmpty(data.texts.loading_metadata)) 
                            texts["loading_metadata"] = data.texts.loading_metadata;
                        if (!string.IsNullOrEmpty(data.texts.launching_game)) 
                            texts["launching_game"] = data.texts.launching_game;
                    }

                    var allFiles = ParseResList(data.files);
                    
                    var resList = new List<Res>();
                    foreach (var r in allFiles)
                    {
                        string localPath = GetLocalPath(r.name);
                        if (!System.IO.File.Exists(localPath))
                        {
                            resList.Add(r);
                        }
                        else
                        {
                            string localMd5 = File.GetFileMd5(localPath);
                            if (localMd5 != r.md5.ToLowerInvariant())
                            {
                                resList.Add(r);
                            }
                        }
                    }

                    callback(resList.Count > 0, resList, texts);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Hot] Failed to parse check response: {ex.Message}\n{ex.StackTrace}");
                    callback(false, null, null);
                }
            });
        }

        [System.Serializable]
        private class CheckResponse
        {
            public string files;
            public CheckTexts texts;
        }

        [System.Serializable]
        private class CheckTexts
        {
            public string comparing;
            public string resources_updated;
            public string start_downloading;
            public string download_complete;
            public string download_failed;
            public string loading_metadata;
            public string launching_game;
        }

        public void Downloads(List<Res> resList, Action<bool, int, string> progressCallback)
        {
            StartCoroutine(DownloadCoroutine(resList, progressCallback));
        }

        private IEnumerator DownloadCoroutine(List<Res> reses, Action<bool, int, string> progress)
        {
            int already = 0;
            int total = reses.Sum(r => r.size);
            const int maxRetry = 3;

            foreach (var res in reses)
            {
                string url = $"{Url}/{Platform}/{res.name}";
                string path = GetLocalPath(res.name);

                int attempts = 0;
                bool success = false;

                while (attempts < maxRetry && !success)
                {
                    bool finished = false;
                    string error = null;
                    int downloaded = 0;

                    Http.Instance.RequestDownload(url, path, (ok, size, err) =>
                    {
                        finished = true;
                        downloaded = size;
                        error = err;
                    });

                    while (!finished) yield return null;

                    if (!string.IsNullOrEmpty(error))
                    {
                        attempts++;
                        yield return new WaitForSeconds(1f);
                    }
                    else
                    {
                        string localMd5 = File.GetFileMd5(path);
                        if (localMd5 != res.md5.ToLowerInvariant())
                        {
                            System.IO.File.Delete(path);
                            attempts++;
                            yield return new WaitForSeconds(0.5f);
                        }
                        else
                        {
                            already += downloaded;
                            progress(false, downloaded, null);
                            success = true;
                        }
                    }
                }

                if (!success)
                {
                    progress(false, 0, res.name);
                    yield break;
                }
            }

            progress(true, 0, null);
        }

        private List<Res> ParseResList(string fileContent)
        {
            return fileContent.Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .Select(line => new Res(line))
                .ToList();
        }

        private string GetLocalPath(string fileName) => $"{FrameworkPath.Runtime}/{fileName}";

        public void LoadAssetDependencies()
        {
            string path = GetLocalPath("resdepend.txt");
            if (System.IO.File.Exists(path))
            {
                string content = System.IO.File.ReadAllText(path);
                AssetManager.Instance.AddDependData(content);
            }
            else
            {
                Debug.LogWarning("resdepend.txt not found");
            }
        }
    }
}
