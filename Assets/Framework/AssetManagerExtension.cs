using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Audio;

namespace Framework
{
    public static class AssetManagerExtension
    {
#if UNITY_EDITOR && !GAME_PACK
        private static string GetSuffix<T>() where T : UnityEngine.Object
        {
            return typeof(T) switch
            {
                var t when t == typeof(Texture) => ".png|.gif",
                var t when t == typeof(TextAsset) => ".bytes",
                var t when t == typeof(GameObject) => ".prefab",
                var t when t == typeof(Sprite) => ".png|.gif",
                var t when t == typeof(AudioMixer) => ".mixer",
                var t when t == typeof(AudioClip) => ".mp3|.ogg|.wav",
                var t when t == typeof(Material) => ".mat",
                _ => ""
            };
        }

        private static string GetFullPath(string path, string name, string suffix)
        {
            string rootPath = $"{FrameworkPath.HotResources}/{path}/{name}";
            foreach (var ext in suffix.Split('|'))
            {
                string fullPath = rootPath + ext;
                if (System.IO.File.Exists(fullPath))
                    return FrameworkPath.ConvertToAssets(fullPath);
            }
            return "";
        }

        public static T SyncLoad<T>(string path, string name) where T : UnityEngine.Object
        {
            string assetPath = GetFullPath(path, name, GetSuffix<T>());
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public static void AsyncLoad<T>(string path, string name, Action<T> callback) where T : UnityEngine.Object
        {
            callback?.Invoke(SyncLoad<T>(path, name));
        }

#else

        public static T SyncLoad<T>(string path, string name) where T : UnityEngine.Object
        {
            return AssetManager.Instance.SyncLoadRes<T>(path, name);
        }

        public static void AsyncLoad<T>(string path, string name, Action<T> callback) where T : UnityEngine.Object
        {
            AssetManager.Instance.AsyncLoadRes(path, name, callback);
        }

#endif

        public static GameObject LoadPrefab(this AssetManager _, string path, string name)
            => SyncLoad<GameObject>(path, name);

        public static Sprite LoadSprite(this AssetManager _, string path, string name)
            => SyncLoad<Sprite>(path, name);

        public static Texture LoadTexture(this AssetManager _, string path, string name)
            => SyncLoad<Texture>(path, name);

        public static AudioClip LoadAudioClip(this AssetManager _, string path, string name)
            => SyncLoad<AudioClip>(path, name);

        public static TextAsset LoadTextAsset(this AssetManager _, string path, string name)
            => SyncLoad<TextAsset>(path, name);

        public static Material LoadMaterial(this AssetManager _, string path, string name)
            => SyncLoad<Material>(path, name);
    }
}
