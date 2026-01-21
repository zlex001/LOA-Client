using Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class AutoAppend
{
    private static void AppendGameManager()
    {
        FreshLayer();
        FreshPb();
        FreshScene();
    }

    private static void FreshLayer()
    {
        string gamePath = Path.Scripts + "/Layer";

        string[] files = Framework.File.GetFiles(gamePath, "meta");
        string content = "\t" + @"LayerName = 
    {" + "\n";
        foreach (var filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);

            if (fileName.EndsWith("Layer"))
            {
                string line = "\t\t[\"" + fileName.Remove(fileName.Length - 5) + "\"] = " + "\"" + fileName.Remove(fileName.Length - 5) + "\"" + "," + "\n";
                content += line;
            }
        }
        content += "\t}";

        //viewName
        content += "\n\t" + @"ViewName = 
    {" + "\n";
        foreach (var filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);

            if (fileName.EndsWith("View"))
            {
                string line = "\t\t[\"" + fileName + "\"] = " + "\"" + fileName + "\"" + "," + "\n";
                content += line;
            }
        }
        content += "\t}";


        content = content.TrimEnd('\n');

        FreshGameManager("--Layer begin", "--Layer end", content);
    }
    private static void FreshPb()
    {
        string gamePath = Path.Res + "/RawAssets/PbFile";

        string tempStr = "\tNetManager.Instance:LoadPbFiles({tempString})";

        string content = "";
        string[] files = Framework.File.GetFiles(gamePath, "meta");
        foreach (var filePath in files)
        {
            if (filePath.Contains(".DS_Store")) {
                continue;
            }
            FileInfo fileInfo = new FileInfo(filePath);

            string fileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);
            string line = "\"" + fileName + "\"" + ",";
            content += line;
        }
        content = content.TrimEnd(',');

        FreshGameManager("--PbList begin", "--PbList end", tempStr.Replace("tempString", content));
    }


    private static void FreshScene()
    {
        string gamePath = Path.Res + "/Scenes";
        string content = "";

        string[] files = Framework.File.GetFiles(gamePath, "meta");

        content += "\t" + @"SceneName =
    {" + "\n";

        foreach (var filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Extension == ".unity")
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);

                string line = "\t\t[\"" + fileName + "\"]" + " = " + "\"" + fileName + "\"" + "," + "\n";
                content += line;

            }
        }
        content += "\t}";

        FreshGameManager("--Scene begin", "--Scene end", content);
    }

    private static void FreshGameManager(string startStr, string endStr, string content)
    {
        string str = System.IO.File.ReadAllText(Path.Scripts + "/Entrance/init.lua");

        int startIndex = str.IndexOf(startStr);
        int endIndex = str.IndexOf(endStr);

        string remove = str.Remove(startIndex + startStr.Length, endIndex - startIndex - startStr.Length);
        string insertStr = remove.Insert(startIndex + startStr.Length, "\n" + content + "\n");

        System.IO.File.WriteAllText(Path.Scripts + "/Entrance/init.lua", insertStr);
    }

}
