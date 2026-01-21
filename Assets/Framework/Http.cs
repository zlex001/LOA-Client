using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Http : Singleton<Http>
{
    private const int TimeoutSeconds = 8;
    private string _acceptLanguage = "zh-CN";

    public string AcceptLanguage
    {
        get => _acceptLanguage;
        set => _acceptLanguage = value;
    }

    public void RequestGet(string url, Action<bool, string> callback) =>
        StartCoroutine(Get(url, callback));

    public void RequestGetWithTag<T>(string url, T tag, Action<bool, string, T> callback) =>
        StartCoroutine(Get(url, tag, callback));

    public void RequestPost(string url, string json, Action<bool, string> callback) =>
        StartCoroutine(Post(url, json, callback));

    public void RequestDownload(string url, string path, Action<bool, int, string> callback)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        StartCoroutine(Download(url, path, callback));
    }

    private IEnumerator Get(string url, Action<bool, string> callback)
    {
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.timeout = TimeoutSeconds;
        req.SetRequestHeader("Accept-Language", _acceptLanguage);
        yield return req.SendWebRequest();

        bool success = req.result == UnityWebRequest.Result.Success;
        callback(success, success ? req.downloadHandler.text : req.error);
    }

    private IEnumerator Get<T>(string url, T tag, Action<bool, string, T> callback)
    {
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.timeout = TimeoutSeconds;
        req.SetRequestHeader("Accept-Language", _acceptLanguage);
        yield return req.SendWebRequest();

        bool success = req.result == UnityWebRequest.Result.Success;
        callback(success, success ? req.downloadHandler.text : req.error, tag);
    }

    private IEnumerator Post(string url, string json, Action<bool, string> callback)
    {
        using UnityWebRequest req = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = TimeoutSeconds
        };
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept-Language", _acceptLanguage);
        yield return req.SendWebRequest();

        bool success = req.result == UnityWebRequest.Result.Success;
        callback(success, success ? req.downloadHandler.text : req.error);
    }

    private IEnumerator Download(string url, string path, Action<bool, int, string> callback)
    {
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.timeout = 10;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            byte[] data = req.downloadHandler.data;
            System.IO.File.WriteAllBytes(path, data);
            callback(true, data.Length, null);
        }
        else
        {
            callback(false, 0, req.error);
        }
    }
}
