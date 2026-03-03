using Game.Data;
using Game.Net.Protocol;
using UnityEngine;

namespace Game.Net.Authentication
{
    /// <summary>
    /// Common module for the Authentication system in Net layer.
    /// Shared state, initialization, and protocol helpers used by Connection and other modules.
    /// </summary>
    public static class Agent
    {
        private static bool _initialized;
        internal static bool PendingLogin;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            DataManager.Instance.after.Register(DataManager.Type.LoginResponse, OnAfterLoginResponseChanged);
            Connection.Init();
        }

        public static void SendQuickStartRequest()
        {
            NetManager.Instance.Send(new QuickStartRequest
            {
                device = DataManager.Instance.Device,
                version = DataManager.Instance.AppVersion,
                platform = Application.platform.ToString(),
                language = DataManager.Instance.Language.ToString()
            });
        }

        public static void SendLoginRequest(Account account)
        {
            Utils.Debug.Log("Auth", "Sending Login protocol");
            NetManager.Instance.Send(new Login(account));
        }

        private static void OnAfterLoginResponseChanged(params object[] args)
        {
            PendingLogin = false;
        }
    }
}
