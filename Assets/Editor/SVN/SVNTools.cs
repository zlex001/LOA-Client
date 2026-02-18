using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class SVNTools
{
    [MenuItem("SVN/Commit")]
    public static void SVNCommit()
    {
        ProcessCommand("TortoiseProc.exe", "/command:commit /path:" + SVNProjectPath);
    }

    [MenuItem("SVN/Update")]
    public static void SVNUpdate()
    {
        ProcessCommand("TortoiseProc.exe", "/command:update /path:" + SVNProjectPath + " /closeonend:1");
    }

    [MenuItem("SVN/CleanUp")]
    static void SVNCleanUp()
    {
        ProcessCommand("TortoiseProc.exe", "/command:cleanup /path:" + SVNProjectPath);
    }

    [MenuItem("SVN/Log")]
    static void SVNLog()
    {
        ProcessCommand("TortoiseProc.exe", "/command:log /path:" + SVNProjectPath);
    }

    /// <summary>
    /// 获取当前项目路径当做SVN本地路径
    /// </summary>
    static string SVNProjectPath
    {
        get
        {
            //System.IO.DirectoryInfo parent = System.IO.Directory.GetParent(Application.dataPath);
            //return parent.ToString();
            return Application.dataPath;
        }
    }

    /// <summary>
    /// 通过Unity进程类调用SVN
    /// </summary>
    /// <param name="command">需要执行的名称</param>
    /// <param name="argument">参数</param>
    public static void ProcessCommand(string command, string argument)
    {

        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command);
        info.Arguments = argument;
        info.CreateNoWindow = false;
        info.ErrorDialog = true;
        info.UseShellExecute = true;

        if (info.UseShellExecute)
        {
            info.RedirectStandardOutput = false;
            info.RedirectStandardError = false;
            info.RedirectStandardInput = false;
        }
        else
        {
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.RedirectStandardInput = true;
            info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }

        System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);

        if (!info.UseShellExecute)
        {
            Debug.Log(process.StandardOutput);
            Debug.Log(process.StandardError);
        }

        process.WaitForExit();
        process.Close();
    }
}
