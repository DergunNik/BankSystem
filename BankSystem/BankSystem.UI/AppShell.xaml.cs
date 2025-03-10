namespace BankSystem.BankClient
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Pages.StartPage), typeof(Pages.StartPage));
            Routing.RegisterRoute(nameof(Pages.AuthPage), typeof(Pages.AuthPage));
        }
    }
}
