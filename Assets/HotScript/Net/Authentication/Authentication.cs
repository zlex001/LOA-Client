using Game.Data;
using Game.Net.Protocol;
using UnityEngine;

namespace Game.Net
{
    /// <summary>
    /// Authentication network coordination.
    /// Manages connection lifecycle and protocol sending for login flows.
    /// </summary>
    public static class Authentication
    {
        private static bool _initialized;
        private static bool _pendingLogin;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            DataManager.Instance.after.Register(DataManager.Type.LoginAccount, OnAfterLoginAccountChanged);
            DataManager.Instance.after.Register(DataManager.Type.Online, OnAfterOnlineChanged);
            DataManager.Instance.after.Register(DataManager.Type.LoginResponse, OnAfterLoginResponseChanged);
        }

        /// <summary>
        /// Called by Presentation.Authentication when user switches accounts.
        /// Disconnect, update selection, persist, and reconnect.
        /// </summary>
        public static void SwitchAccount(Account account, int accountIndex)
        {
            Utils.Debug.Log("Auth", $"SwitchAccount to index {accountIndex}, account: {account.Id}");
            _pendingLogin = true;

            DataManager.Instance.User.SelectedAccountIndex = accountIndex;
            Local.Instance.Save(DataManager.Instance.User);

            NetManager.Instance.Disconnect();
            NetManager.Instance.Connect(
                DataManager.Instance.SelectedServer.Ip,
                DataManager.Instance.SelectedServer.Port
            );
        }

        #region Monitor Handlers

        private static void OnAfterLoginAccountChanged(params object[] args)
        {
            Account account = args[0] as Account;
            Utils.Debug.Log("Auth", $"LoginAccount changed. Online: {DataManager.Instance.Online}");

            if (account == null)
            {
                Utils.Debug.LogWarning("Auth", "LoginAccount is null, skipping");
                return;
            }

            _pendingLogin = true;

            if (DataManager.Instance.Online)
            {
                if (account.Id == "__QuickStart__")
                {
                    SendQuickStartRequest();
                }
                else
                {
                    Utils.Debug.Log("Auth", "Online, sending Login protocol");
                    NetManager.Instance.Send(new Login(account));
                }
            }
            else
            {
                var server = DataManager.Instance.SelectedServer;
                Utils.Debug.Log("Auth", $"Not online, connecting to {server.Ip}:{server.Port}");
                NetManager.Instance.Connect(server.Ip, server.Port);
            }
        }

        private static void OnAfterOnlineChanged(params object[] args)
        {
            bool isOnline = (bool)args[0];
            if (!isOnline || !_pendingLogin) return;

            var loginAccount = DataManager.Instance.LoginAccount;
            if (loginAccount == null) return;

            if (loginAccount.Id == "__QuickStart__")
            {
                Utils.Debug.Log("Auth", "Connected, sending QuickStartRequest");
                SendQuickStartRequest();
            }
            else
            {
                Utils.Debug.Log("Auth", "Connected, sending Login protocol");
                NetManager.Instance.Send(new Login(DataManager.Instance.SelectedAccount));
            }
        }

        private static void OnAfterLoginResponseChanged(params object[] args)
        {
            _pendingLogin = false;
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}
