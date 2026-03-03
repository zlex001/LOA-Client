using System.Linq;
using Game.Net.Protocol;

namespace Game.Data
{
    public partial class DataManager
    {
        #region Auth Properties

        public Account SelectedAccount => User.Accounts[User.SelectedAccountIndex];

        public Account LoginAccount
        {
            get => Get<Account>(Type.LoginAccount);
            set => Change(Type.LoginAccount, value);
        }

        public int LoginResponse
        {
            get => Get<int>(Type.LoginResponse);
            set => Change(Type.LoginResponse, value);
        }

        public string LoginResponseMessage
        {
            get => Get<string>(Type.LoginResponseMessage);
            set => Change(Type.LoginResponseMessage, value);
        }

        /// <summary>
        /// Temporary storage for guest account data from LoginResponse.
        /// Set by Protocol.LoginResponse.Processed(), consumed and cleared by OnAfterLoginResponseChanged.
        /// Not reactive -- no Monitor events, just a pass-through container.
        /// </summary>
        public LoginResponseAccountData LoginResponseAccountData { get; set; }

        #endregion

        #region Auth Initialization

        private void RegisterAuthMonitors()
        {
            after.Register(Type.LoginAccount, OnAfterLoginAccountChanged);
            after.Register(Type.LoginResponse, OnAfterLoginResponseChanged);
        }

        #endregion

        #region Auth Monitor Handlers

        private void OnAfterLoginAccountChanged(params object[] args)
        {
            Account account = (Account)args[0];
            if (!User.Accounts.Contains(account))
            {
                User.Accounts.Add(account);
                User.SelectedAccountIndex = User.Accounts.IndexOf(account);
                Local.Instance.Save(User);
            }
        }

        private void OnAfterLoginResponseChanged(params object[] args)
        {
            int code = (int)args[0];
            if (code != (int)LoginResponse.Code.Success) return;

            var responseData = LoginResponseAccountData;
            if (responseData == null) return;

            if (string.IsNullOrEmpty(responseData.AccountId) ||
                string.IsNullOrEmpty(responseData.Password))
                return;

            bool accountExists = User.Accounts.Any(a => a.Id == responseData.AccountId);

            if (!accountExists)
            {
                var account = new Account
                {
                    Id = responseData.AccountId,
                    Password = responseData.Password,
                    Note = responseData.IsGuest ? "Guest Account" : "Bound Account"
                };
                User.Accounts.Add(account);
                User.SelectedAccountIndex = User.Accounts.Count - 1;
                Local.Instance.Save(User);
                Utils.Debug.Log("Auth",
                    $"Saved guest account: {responseData.AccountId}, isNew: {responseData.IsNewAccount}");
            }
            else
            {
                Utils.Debug.Log("Auth",
                    $"Guest account already exists: {responseData.AccountId}");
            }

            LoginResponseAccountData = null;
        }

        #endregion
    }
}
