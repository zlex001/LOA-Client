using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : Singleton<AssetManager>
{
    public class AssetInfo
    {
        public AssetBundle ab;
        public int count = 0;
    }

    private Dictionary<string, AssetInfo> assetDict = new();
    private Dictionary<string, List<string>> dependDict = new();

    private string NormalizeBundleName(string abName) => abName.Replace("/", "_").ToLower();

    private string GetBundlePath(string abName) => FrameworkPath.HotResources + "/" + NormalizeBundleName(abName);

    public void AddDependData(string assetJsonStr)
    {
        AssetDependData assetDependData = JsonUtility.FromJson<AssetDependData>(assetJsonStr);
        dependDict = assetDependData.ToDictionary();
    }

    public T SyncLoadRes<T>(string abName, string resName) where T : UnityEngine.Object
    {
        abName = NormalizeBundleName(abName);
        SyncLoadAssetBundle(abName);
        AddReferCount(abName);

        return assetDict.TryGetValue(abName, out var info)
            ? info.ab.LoadAsset<T>(resName)
            : null;
    }

    public void SyncLoadAssetBundle(string abName)
    {
#if UNITY_EDITOR
        return;
#endif
        abName = NormalizeBundleName(abName);
        string path = GetBundlePath(abName);
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"AssetBundle 不存在: {path}");
            return;
        }

        if (assetDict.ContainsKey(abName)) return;

        var ab = AssetBundle.LoadFromFile(path);
        assetDict[abName] = new AssetInfo { ab = ab };

        if (dependDict.TryGetValue(abName, out var dependencies))
        {
            foreach (var dep in dependencies)
                SyncLoadAssetBundle(dep);
        }
    }

    public void AsyncLoadRes<T>(string abName, string resName, Action<T> callback) where T : UnityEngine.Object
    {
        StartCoroutine(AsyncLoadAssetBundle(abName, success =>
        {
            if (success)
            {
                StartCoroutine(AsyncLoadAsset(abName, resName, obj =>
                {
                    callback?.Invoke(obj as T);
                    AddReferCount(abName);
                }));
            }
            else callback?.Invoke(null);
        }));
    }

    private IEnumerator AsyncLoadAssetBundle(string abName, Action<bool> callback)
    {
        abName = NormalizeBundleName(abName);
        string path = GetBundlePath(abName);
        if (!System.IO.File.Exists(path))
        {
            callback?.Invoke(false);
            yield break;
        }

        if (assetDict.ContainsKey(abName))
        {
            callback?.Invoke(true);
            yield break;
        }

        var request = AssetBundle.LoadFromFileAsync(path);
        yield return request;

        if (request.assetBundle == null)
        {
            callback?.Invoke(false);
            yield break;
        }

        assetDict[abName] = new AssetInfo { ab = request.assetBundle };
        if (dependDict.TryGetValue(abName, out var dependencies))
        {
            foreach (var dep in dependencies)
            {
                if (!assetDict.ContainsKey(dep))
                    yield return AsyncLoadAssetBundle(dep, null);
            }
        }

        callback?.Invoke(true);
    }

    private IEnumerator AsyncLoadAsset(string abName, string resName, Action<UnityEngine.Object> callback)
    {
        abName = NormalizeBundleName(abName);
        var request = assetDict[abName].ab.LoadAssetAsync(resName);
        yield return request;
        callback?.Invoke(request.asset);
    }

    public void UnloadRes(string abName) => MinusReferCount(abName, new HashSet<string>());

    private void AddReferCount(string abName)
    {
        if (!assetDict.ContainsKey(abName)) return;
        assetDict[abName].count++;

        if (dependDict.TryGetValue(abName, out var dependencies))
        {
            foreach (var dep in dependencies)
                AddReferCount(dep);
        }
    }

    private void MinusReferCount(string abName, HashSet<string> visited)
    {
        abName = NormalizeBundleName(abName);
        if (!assetDict.ContainsKey(abName) || visited.Contains(abName)) return;

        visited.Add(abName);
        if (assetDict[abName].count > 0)
            assetDict[abName].count--;

        if (dependDict.TryGetValue(abName, out var dependencies))
        {
            foreach (var dep in dependencies)
                MinusReferCount(dep, visited);
        }
    }

    public void ClearFreeRes()
    {
        var keysToRemove = new List<string>();
        foreach (var kv in assetDict)
        {
            if (kv.Value.count <= 0)
            {
                kv.Value.ab.Unload(false);
                keysToRemove.Add(kv.Key);
            }
        }

        foreach (var key in keysToRemove)
            assetDict.Remove(key);

        Resources.UnloadUnusedAssets();
    }
}
