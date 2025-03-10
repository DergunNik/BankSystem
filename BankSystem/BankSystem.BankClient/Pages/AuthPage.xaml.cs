using BankSystem.BankClient.ViewModels;

namespace BankSystem.BankClient.Pages;

public partial class AuthPage : ContentPage
{
	public AuthPage(AuthViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}