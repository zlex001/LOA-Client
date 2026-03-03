using Game.Data;
using Game.Net.Protocol;
using UnityEngine;

namespace Game.Net
{
    public partial class NetManager
    {
        #region Auth Fields

        private bool _pendingLogin;

        #endregion

        #region Auth Initialization

        private void RegisterAuthMonitors()
        {
            DataManager.Instance.after.Register(DataManager.Type.LoginAccount, OnAuthLoginAccountChanged);
            DataManager.Instance.after.Register(DataManager.Type.LoginResponse, OnAuthLoginResponseChanged);
        }

        #endregion

        #region Auth Public Methods

        /// <summary>
        /// Disconnect current connection, update account selection, and reconnect.
        /// Called by Presentation when user switches accounts.
        /// </summary>
        public void SwitchAccount(Account account, int accountIndex)
        {
            Utils.Debug.Log("Auth", $"SwitchAccount to index {accountIndex}, account: {account.Id}");
            _pendingLogin = true;

            DataManager.Instance.User.SelectedAccountIndex = accountIndex;
            Local.Instance.Save(DataManager.Instance.User);

            Disconnect();
            DataManager.Instance.LoginAccount = account;
        }

        #endregion

        #region Auth Monitor Handlers

        private void OnAuthLoginAccountChanged(params object[] args)
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
                    Send(new Login(account));
                }
            }
            else
            {
                var server = DataManager.Instance.SelectedServer;
                Utils.Debug.Log("Auth", $"Not online, connecting to {server.Ip}:{server.Port}");
                Connect(server.Ip, server.Port);
            }
        }

        /// <summary>
        /// When connection is established and there is a pending login,
        /// send the appropriate login protocol.
        /// Called from the existing OnAfterOnlineChanged in NetManager.cs.
        /// </summary>
        private void OnAuthOnlineEstablished()
        {
            if (!_pendingLogin) return;

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
                Send(new Login(DataManager.Instance.SelectedAccount));
            }
        }

        private void OnAuthLoginResponseChanged(params object[] args)
        {
            _pendingLogin = false;
        }

        #endregion

        #region Auth Private Methods

        private void SendQuickStartRequest()
        {
            Send(new QuickStartRequest
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
