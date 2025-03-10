namespace BankSystem.BankClient
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Pages.BanksPage), typeof(Pages.BanksPage));
            Routing.RegisterRoute(nameof(Pages.AuthPage), typeof(Pages.AuthPage));
        }
    }
}
