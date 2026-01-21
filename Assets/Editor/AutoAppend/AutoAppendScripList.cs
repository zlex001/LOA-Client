using Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class AutoAppend
{
    private static void AppendLayer()
    {
        string gamePath = Path.Scripts + "/Layer";

        string content = "";
        string[] files = Framework.File.GetFiles(gamePath, "meta");
        foreach (var filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string localPath = System.IO.Path.Combine(fileInfo.FullName).Remove(0, Path.Scripts.Length + 1);
            
            localPath = System.IO.Path.Combine(localPath).Replace("\\",".");

            string fileName = System.IO.Path.GetFileNameWithoutExtension(localPath);
            //+"\"" + fileName + "\"" + " = " +
            content += "require(" + "\"" + localPath.Remove(localPath.Length - 4, 4) + "\")" + "\n";
        }
        content = content.TrimEnd('\n');
        FreshScripList("--Layer begin", "--Layer end", content);
    }


    public static void FreshExcel()
    {
        string content = "";
        string[] files = Framework.File.GetFiles(Path.Scripts + "/Data", "meta");
        foreach (var filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string localPath = System.IO.Path.Combine(fileInfo.FullName).Remove(0, Path.Scripts.Length + 1).Replace("\\", "."); ;

            string fileName = System.IO.Path.GetFileNameWithoutExtension(localPath);
            //+"\"" + fileName + "\"" + " = " +
            content += "require(" + "\"" + localPath.Remove(localPath.Length - 4, 4) + "\")" + "\n";
        }
        content = content.TrimEnd('\n');

        FreshScripList("--Config begin", "--Config end", content);
    }

    public static void FreshSceneScript()
    {
        string content = "";
        string[] files = Framework.File.GetFiles(Path.Scripts + "/Scene", "meta");
        foreach (var filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string localPath = System.IO.Path.Combine(fileInfo.FullName).Remove(0, Path.Scripts.Length + 1).Replace("\\", "."); ;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(localPath);
            //+"\"" + fileName + "\"" + " = " +
            content += "require(" + "\"" + localPath.Remove(localPath.Length - 4, 4) + "\")" + "\n";
        }
        content = content.TrimEnd('\n');

        FreshScripList("--Scene begin", "--Scene end", content);
    }

    private static void FreshScripList(string startStr, string endStr, string content)
    {
        string str = System.IO.File.ReadAllText(Path.Scripts + "/Entrance/ScripList.lua");

        int startIndex = str.IndexOf(startStr);
        int endIndex = str.IndexOf(endStr);

        string remove = str.Remove(startIndex + startStr.Length, endIndex - startIndex - startStr.Length);
        string insertStr = remove.Insert(startIndex + startStr.Length, "\n" + content + "\n");

        System.IO.File.WriteAllText(Path.Scripts + "/Entrance/ScripList.lua", insertStr);
    }
}
