using UnityEngine;
using System.IO;

namespace Framework
{
    public static class FrameworkPath
    {
        private static string Normalize(string path) => path.Replace("\\", "/");

        public static string Runtime =>
#if UNITY_EDITOR
            Normalize(Application.dataPath);
#else
            Normalize(Application.persistentDataPath + "/Game");
#endif

        public static string Package =>
#if UNITY_EDITOR || UNITY_ANDROID
            Normalize(Application.streamingAssetsPath);
#else
            "file://" + Normalize(Application.streamingAssetsPath);
#endif

        public static string Scripts =>
#if UNITY_EDITOR
            Normalize(Application.dataPath + "/HotScript");
#else
            Runtime + "/scripts";
#endif

        public static string HotResources =>
#if UNITY_EDITOR
            Normalize(Application.dataPath + "/HotBundle");
#else
            Runtime + "/res";
#endif

        public static string Storage =>
#if UNITY_EDITOR
            Normalize(Application.dataPath + "/../Storage");
#else
            Runtime + "/Storage";
#endif

        public static string ConvertToAssets(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            path = Normalize(path);
            int index = path.IndexOf("Assets/");
            return index >= 0 ? path.Substring(index) : string.Empty;
        }
    }
}
