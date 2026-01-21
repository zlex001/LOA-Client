using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class Bat
{
    public static void Run(string batFile, string workingDir)
    {
        string path = System.IO.Path.Combine(workingDir, batFile);
        using var proc = new Process { StartInfo = new ProcessStartInfo { WorkingDirectory = workingDir, FileName = batFile } };
        proc.Start();
        proc.WaitForExit();
    }
}
