using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class AutoAppend : MonoBehaviour
{

    [MenuItem("Tools/自动补全Lua")]
    public static void Append()
    {
        AppendLayer();
        FreshSceneScript();
        FreshExcel();
        AppendGameManager();
        Debug.Log("自动补全Lua完成");
    }

}
