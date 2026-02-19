using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    public static class Config
    {
        public class UI
        {
            public static (string, string, int, bool) Effect = ("Prefabs/UI", "Effect", 0, false);
            public static (string, string, int, bool) Start = ("Prefabs/UI", "Start", 0, false);
            public static (string, string, int, bool) StartSettings = ("Prefabs/UI", "StartSettings", 1, true);
            public static (string, string, int, bool) Login = ("Prefabs/UI", "Login", 0, false);
            public static (string, string, int, bool) Account = ("Prefabs/UI", "Account", 1, false);
            public static (string, string, int, bool) Initialize = ("Prefabs/UI", "Initialize", 0, false);
            public static (string, string, int, bool) Input = ("Prefabs/UI", "Input", 0, false);
            public static (string, string, int, bool) Home = ("Prefabs/UI", "Home", 1, false);
            public static (string, string, int, bool) Option = ("Prefabs/UI", "Option", 2, true);
            public static (string, string, int, bool) Story = ("Prefabs/UI", "Story", 3, false);
            public static (string, string, int, bool) Tutorial = ("Prefabs/UI", "Tutorial", 4, true);
            public static (string, string, int, bool) Dark = ("Prefabs/UI", "Dark", 5, false);
        }
        public static class Audio
        {
            public static (string, string, bool, bool) BGM_Town = ("Audio/BGM", "BGM_Town", true, true);
            public static (string, string, bool, bool) SFX_Attack_Slash = ("Audio/SFX", "Attack_Slash", false, false);
            public static (string, string, bool, bool) Click = ("Audio/UI", "Click", false, false);
            public static (string, string, bool, bool) OptionOpen = ("Audio/UI", "OptionOpen", false, false);
            public static (string, string, bool, bool) OptionClose = ("Audio/UI", "OptionClose", false, false);
        }
        public static class Net
        {
            public static float HeartbeatInterval => 4f;
            public static int MaxMissedHeartbeats => 3;
            public static float HeartbeatTimeout => HeartbeatInterval + 2f;
            public static int SocketPollIntervalMs => 5;
            public static int SocketBlockSize => 1024;
            public static int SocketBufferSize => 1024 * 128;
            public static int ConnectionTimeoutMs = 5000;
            public static bool EnableAutoReconnect => true;
            public static float InitialReconnectDelay => 1f;
            public static float MaxReconnectDelay => 30f;
        }
        public static int VerticalScreenTextCount = 32;
        public static int VerticalScreenButtonCount = 18;
        public static float VerticalScreenButtonHeight => Screen.height / VerticalScreenButtonCount;
    }
}
