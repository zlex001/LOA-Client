using Game.Data;

namespace Game.Net.Authentication
{
    public static class Connection
    {
        internal static void Init()
        {
            DataManager.Instance.after.Register(DataManager.Type.LoginAccount, OnAfterLoginAccountChanged);
            DataManager.Instance.after.Register(DataManager.Type.Online, OnAfterOnlineChanged);
        }

        public static void SwitchAccount(Account account, int accountIndex)
        {
            Utils.Debug.Log("Auth", $"SwitchAccount to index {accountIndex}, account: {account.Id}");
            Agent.PendingLogin = true;

            DataManager.Instance.User.SelectedAccountIndex = accountIndex;
            Local.Instance.Save(DataManager.Instance.User);

            NetManager.Instance.Disconnect();
            NetManager.Instance.Connect(
                DataManager.Instance.SelectedServer.Ip,
                DataManager.Instance.SelectedServer.Port
            );
        }

        private static void OnAfterLoginAccountChanged(params object[] args)
        {
            Account account = args[0] as Account;
            Utils.Debug.Log("Auth", $"LoginAccount changed. Online: {DataManager.Instance.Online}");

            if (account == null)
            {
                Utils.Debug.LogWarning("Auth", "LoginAccount is null, skipping");
                return;
            }

            Agent.PendingLogin = true;

            if (DataManager.Instance.Online)
            {
                Agent.SendLoginRequest(account);
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
            if (!isOnline || !Agent.PendingLogin) return;

            var loginAccount = DataManager.Instance.LoginAccount;
            if (loginAccount == null) return;

            Utils.Debug.Log("Auth", "Connected, sending Login");
            Agent.SendLoginRequest(loginAccount);
        }
    }
}
