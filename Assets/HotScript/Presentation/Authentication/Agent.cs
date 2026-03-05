using Game.Data;

namespace Game.Presentation.Authentication
{
    public static class Agent
    {
        public static void Login(Account account)
        {
            Utils.Debug.Log("Auth", $"Login requested for account: {account.Id}");
            DataManager.Instance.LoginAccount = account;
        }

        public static void QuickStart()
        {
            Utils.Debug.Log("Auth", "QuickStart requested");

            if (!DataManager.Instance.Online)
            {
                DataManager.Instance.LoginAccount = new Account();
            }
            else
            {
                Net.Authentication.Agent.SendLoginRequest(null);
            }
        }

        public static void SwitchAccount(Account account, int accountIndex)
        {
            Net.Authentication.Connection.SwitchAccount(account, accountIndex);
        }
    }
}
