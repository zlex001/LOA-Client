using Game.Data;

namespace Game.Presentation
{
    /// <summary>
    /// Authentication public API for UI classes.
    /// Start.cs, AccountUI.cs, StartSettings call these methods.
    /// Callers are responsible for their own UI feedback (e.g. Dark overlay).
    /// </summary>
    public static class Authentication
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
                DataManager.Instance.LoginAccount = new Account { Id = "__QuickStart__" };
            }
            else
            {
                Net.Authentication.SendQuickStartRequest();
            }
        }

        public static void SwitchAccount(Account account, int accountIndex)
        {
            Net.Authentication.SwitchAccount(account, accountIndex);
        }
    }
}
