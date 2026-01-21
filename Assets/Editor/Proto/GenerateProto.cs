using Framework;
using UnityEditor;
using UnityEngine;

public class GenerateProto : MonoBehaviour
{
    [MenuItem("Tools/Generate Proto")]
    public static void Start()
    {
        Bat.Run("BuildProto.bat", Path.Utils + "/Protoc3.18/");

        string protoPath = Path.Utils + "/Protoc3.18/Output/Csharp";

        DirectoryUtils.CopyDirectory(protoPath, Path.Scripts + "/World/PbFile");
    }
}
