using BankSystem.BankClient.ViewModels;

namespace BankSystem.BankClient.Pages;

public partial class BanksPage : ContentPage
{
	public BanksPage(BanksViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}