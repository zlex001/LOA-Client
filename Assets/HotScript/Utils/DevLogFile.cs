using System;
using System.IO;
using UnityEngine;

namespace Game.Utils
{
    /// <summary>
    /// In Editor or Development build, forwards all Unity log to Client/Logs/player_latest.log
    /// so tools or AI can read it from the workspace.
    /// </summary>
    public static class DevLogFile
    {
        private static StreamWriter _writer;
        private static readonly object _lock = new object();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
#if UNITY_EDITOR
            Start();
#else
            if (Debug.isDebugBuild)
                Start();
#endif
        }

        private static void Start()
        {
            try
            {
                var logDir = Path.Combine(Application.dataPath, "..", "Logs");
                var logPath = Path.Combine(logDir, "player_latest.log");
                Directory.CreateDirectory(logDir);
                var stream = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(stream, System.Text.Encoding.UTF8) { AutoFlush = true };
                Application.logMessageReceived += OnLogMessageReceived;
            }
            catch
            {
                // Dev log optional
            }
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (_writer == null) return;
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{type}] {condition}";
            if (!string.IsNullOrEmpty(stackTrace))
                line += "\n" + stackTrace;
            lock (_lock)
            {
                try
                {
                    _writer.WriteLine(line);
                }
                catch { }
            }
        }
    }
}
