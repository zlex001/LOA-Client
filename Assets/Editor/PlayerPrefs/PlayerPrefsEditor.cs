using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditor
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void Start()
    {
        PlayerPrefs.DeleteAll();
    }
}
