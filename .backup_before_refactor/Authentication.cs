using Game.Data;
using Game.Net;
using Game.Net.Protocol;
using UnityEngine;

namespace Game.Logic
{
    /// <summary>
    /// Centralizes login/account business logic.
    /// Mirrors the server's Domain.Authentication module.
    /// Presentation layer calls these methods instead of accessing Net/Data directly.
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
        /// Login with an existing account. Called by Presentation layer.
        /// </summary>
        public static void Login(Account account)
        {
            Utils.Debug.Log("Auth", $"Login requested for account: {account.Id}");
            _pendingLogin = true;
            DataManager.Instance.LoginAccount = account;
        }

        /// <summary>
        /// Quick start (guest login). Called by Presentation layer.
        /// </summary>
        public static void QuickStart()
        {
            Utils.Debug.Log("Auth", "QuickStart requested");
            _pendingLogin = true;

            if (!DataManager.Instance.Online)
            {
                var server = DataManager.Instance.SelectedServer;
                Utils.Debug.Log("Auth", $"Not online, connecting to {server.Ip}:{server.Port} for QuickStart");
                DataManager.Instance.LoginAccount = new Account { Id = "__QuickStart__" };
            }
            else
            {
                SendQuickStartRequest();
            }
        }

        /// <summary>
        /// Switch to a different account. Called by Presentation layer.
        /// Handles local persistence, disconnect, and reconnect.
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

        #region DataManager Event Handlers

        private static void OnAfterLoginAccountChanged(params object[] args)
        {
            Account account = args[0] as Account;
            Utils.Debug.Log("Auth", $"LoginAccount changed. Online: {DataManager.Instance.Online}");

            if (account == null)
            {
                Utils.Debug.LogWarning("Auth", "LoginAccount is null, skipping");
                return;
            }

            if (DataManager.Instance.Online)
            {
                if (account.Id == "__QuickStart__")
                {
                    SendQuickStartRequest();
                }
                else
                {
                    Utils.Debug.Log("Auth", "Online, sending Login protocol");
                    NetManager.Instance.Send(new Net.Protocol.Login(account));
                }
            }
            else
            {
                var server = DataManager.Instance.SelectedServer;
                Utils.Debug.Log("Auth", $"Not online, connecting to {server.Ip}:{server.Port}");
                NetManager.Instance.Connect(server.Ip, server.Port);
            }
        }

        /// <summary>
        /// Only sends login when the connection was initiated by an Authentication action
        /// (Login/QuickStart/SwitchAccount), not by auto-reconnect.
        /// </summary>
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
                NetManager.Instance.Send(new Net.Protocol.Login(DataManager.Instance.SelectedAccount));
            }
        }

        private static void OnAfterLoginResponseChanged(params object[] args)
        {
            _pendingLogin = false;

            int code = (int)args[0];
            if (code != (int)LoginResponse.Code.Success) return;

            var responseData = DataManager.Instance.LoginResponseAccountData;
            if (responseData == null) return;

            if (string.IsNullOrEmpty(responseData.AccountId) ||
                string.IsNullOrEmpty(responseData.Password))
                return;

            bool accountExists = false;
            foreach (var existing in DataManager.Instance.User.Accounts)
            {
                if (existing.Id == responseData.AccountId)
                {
                    accountExists = true;
                    break;
                }
            }

            if (!accountExists)
            {
                var account = new Account
                {
                    Id = responseData.AccountId,
                    Password = responseData.Password,
                    Note = responseData.IsGuest ? "Guest Account" : "Bound Account"
                };
                DataManager.Instance.User.Accounts.Add(account);
                DataManager.Instance.User.SelectedAccountIndex =
                    DataManager.Instance.User.Accounts.Count - 1;
                Local.Instance.Save(DataManager.Instance.User);
                Utils.Debug.Log("Auth",
                    $"Saved guest account: {responseData.AccountId}, isNew: {responseData.IsNewAccount}");
            }
            else
            {
                Utils.Debug.Log("Auth",
                    $"Guest account already exists: {responseData.AccountId}");
            }

            DataManager.Instance.LoginResponseAccountData = null;
        }

        #endregion

        private static void SendQuickStartRequest()
        {
            NetManager.Instance.Send(new QuickStartRequest
            {
                device = DataManager.Instance.Device,
                version = DataManager.Instance.AppVersion,
                platform = Application.platform.ToString(),
                language = DataManager.Instance.Language.ToString()
            });
        }
    }
}
