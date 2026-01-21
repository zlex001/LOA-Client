using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CopyPath : Editor
{
    [MenuItem("Custom/ shift + p copy #p")]
    static void CopyResPath()
    {
        var selectObj = Selection.activeObject;

        var path = AssetDatabase.GetAssetPath(selectObj);

        string p = path.Replace("Assets/Game/Res/","");
        
        p = p.Substring(0,p.LastIndexOf("."));
        
        GUIUtility.systemCopyBuffer = p;

        Debug.Log(p);
    }
}
