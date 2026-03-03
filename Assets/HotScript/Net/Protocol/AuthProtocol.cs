using Game.Data;
using UnityEngine;

namespace Game.Net.Protocol
{
    public class Login : Base
    {
        public Login(Account account)
        {
            id = account.Id;
            password = account.Password;
            version = DataManager.Instance.AppVersion;
            platform = Application.platform.ToString();
            device = PlayerPrefs.GetString("DEVICE");
            language = DataManager.Instance.Language.ToString();
            Utils.Debug.Log("Lang", $"Login protocol: sending language=\"{language}\"");
        }
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
        public string message;
        public string accountId;
        public string password;
        public bool isGuest;
        public bool isNewAccount;

        public override void Processed()
        {
            Utils.Debug.Log("Protocol", $"LoginResponse.Processed() called: code={code}, message={message}");

            if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(password))
            {
                DataManager.Instance.LoginResponseAccountData = new LoginResponseAccountData
                {
                    AccountId = accountId,
                    Password = password,
                    IsGuest = isGuest,
                    IsNewAccount = isNewAccount
                };
            }

            DataManager.Instance.LoginResponseMessage = message;
            DataManager.Instance.LoginResponse = code;
        }
    }

    public class QuickStartRequest : Base
    {
        public string device;
        public string version;
        public string platform;
        public string language;
    }
}
