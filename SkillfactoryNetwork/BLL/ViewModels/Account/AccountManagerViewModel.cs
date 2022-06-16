namespace SkillfactoryNetwork.BLL.ViewModels.Account
{
    public class AccountManagerViewModel
    {
        public LoginViewModel LoginView { get; set; }
        public RegisterViewModel RegisterView { get; set; }

        public AccountManagerViewModel()
        {
            LoginView = new LoginViewModel();
            RegisterView = new RegisterViewModel();
        }
    }
}
