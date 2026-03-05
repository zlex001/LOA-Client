using Game.Data;
using Game.Net.Protocol;
using UnityEngine;

namespace Game.Net.Authentication
{
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

        public static void SendLoginRequest(Account account)
        {
            Utils.Debug.Log("Auth", $"Sending LoginRequest, id={account?.Id ?? "(quick)"}");
            NetManager.Instance.Send(new LoginRequest
            {
                id = account?.Id,
                password = account?.Password,
                version = DataManager.Instance.AppVersion,
                platform = Application.platform.ToString(),
                device = DataManager.Instance.Device,
                language = DataManager.Instance.Language.ToString()
            });
        }

        private static void OnAfterLoginResponseChanged(params object[] args)
        {
            PendingLogin = false;
        }
    }
}
