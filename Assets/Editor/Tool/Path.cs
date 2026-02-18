using System.IO;
using UnityEditor;
using UnityEngine;

public class Path
{
    public static string Utils { get { return System.IO.Path.Combine(Application.dataPath + "/../Utils"); } }
    public static string Scripts { get { return System.IO.Path.Combine(Application.dataPath + "/HotScript"); } }
    public static string Res { get { return System.IO.Path.Combine(Application.dataPath + "/HotBundle"); } }
    public static string Artifacts { get { return System.IO.Path.Combine(Application.dataPath + "/../" + "Artifacts"); } }
    public static string Excel { get { return System.IO.Path.Combine(Application.dataPath + "/../Tools/Excel"); } }

    public static string PlatformName
    {
        get
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return "android";
                case BuildTarget.iOS:
                    return "ios";
            }
            return "windows";
        }
    }
}
