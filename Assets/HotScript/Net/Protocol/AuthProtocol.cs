using System.Linq;
using Game.Data;
using UnityEngine;

namespace Game.Net.Protocol
{
    public class Login : Base
    {
        public string id;
        public string password;
        public string platform;
        public string version;
        public string device;
        public string language;
    }

    public class LoginResponse : Base
    {
        public enum Code
        {
            Success,
            PasswordError,
            AppVersionUnfit,
            UnsafeAccount
        }
        public int code;
        public string accountId;
        public string password;
        public bool isGuest;
        public bool isNewAccount;

        public override void Processed()
        {
            Utils.Debug.Log("Protocol", $"LoginResponse.Processed() called: code={code}");

            if (code == (int)Code.Success)
            {
                SaveAccountIfNeeded();
            }

            DataManager.Instance.LoginResponse = code;
        }

        private void SaveAccountIfNeeded()
        {
            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(password))
                return;

            var user = DataManager.Instance.User;
            bool accountExists = user.Accounts.Any(a => a.Id == accountId);

            if (!accountExists)
            {
                var account = new Account
                {
                    Id = accountId,
                    Password = password,
                    Note = isGuest ? "Guest Account" : "Bound Account"
                };
                user.Accounts.Add(account);
                user.SelectedAccountIndex = user.Accounts.Count - 1;
                Local.Instance.Save(user);
                Utils.Debug.Log("Auth",
                    $"Saved guest account: {accountId}, isNew: {isNewAccount}");
            }
        }
    }
}
