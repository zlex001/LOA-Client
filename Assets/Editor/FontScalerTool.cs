using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Framework;

public class FontScalerTool
{
    [MenuItem("Tools/Font/Add FontScaler to All UI Prefabs")]
    static void AddFontScalerToAllPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/HotResources/Prefabs/UI" });
        int totalAdded = 0;
        int prefabsModified = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            bool modified = false;

            foreach (Text text in prefab.GetComponentsInChildren<Text>(true))
            {
                if (text.GetComponent<FontScaler>() == null)
                {
                    text.gameObject.AddComponent<FontScaler>();
                    modified = true;
                    totalAdded++;
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                prefabsModified++;
            }
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        AssetDatabase.Refresh();
        Debug.Log($"FontScaler added to {totalAdded} Text components across {prefabsModified} prefabs.");
    }

    [MenuItem("Tools/Font/Remove FontScaler from All UI Prefabs")]
    static void RemoveFontScalerFromAllPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/HotResources/Prefabs/UI" });
        int totalRemoved = 0;
        int prefabsModified = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            bool modified = false;

            foreach (FontScaler scaler in prefab.GetComponentsInChildren<FontScaler>(true))
            {
                Object.DestroyImmediate(scaler);
                modified = true;
                totalRemoved++;
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                prefabsModified++;
            }
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        AssetDatabase.Refresh();
        Debug.Log($"FontScaler removed from {totalRemoved} Text components across {prefabsModified} prefabs.");
    }
}

