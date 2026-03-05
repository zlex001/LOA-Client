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

        #endregion

        #region Auth Initialization

        private void RegisterAuthMonitors()
        {
            after.Register(Type.LoginAccount, OnAfterLoginAccountChanged);
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

        #endregion
    }
}
