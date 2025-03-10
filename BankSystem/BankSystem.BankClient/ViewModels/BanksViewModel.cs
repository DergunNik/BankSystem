using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BankSystem.BankClient.Services;
using BankSystem.BankClient.Models;
using BankSystem.BankClient.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BankSystem.BankClient.ViewModels
{
    public partial class BanksViewModel : ObservableObject
    {
        private readonly IBankService _bankService;

        public ObservableCollection<Bank> Banks { get; set; } = new();

        [ObservableProperty]
        private bool isBanksLoaded;

        [ObservableProperty]
        private bool isBanksLoadFailed;

        [ObservableProperty]
        private Bank? selectedBank;

        public BanksViewModel(IBankService bankService)
        {
            _bankService = bankService;
            LoadBanksCommand = new AsyncRelayCommand(LoadBanksAsync);
            SelectBankCommand = new AsyncRelayCommand<Bank>(SelectBankAsync);
        }

        public IAsyncRelayCommand LoadBanksCommand { get; }
        public IAsyncRelayCommand<Bank> SelectBankCommand { get; }

        private async Task LoadBanksAsync()
        {
            IsBanksLoaded = false;
            IsBanksLoadFailed = false;

            try
            {
                var banks = await _bankService.GetBanksAsync();
                Banks.Clear();
                foreach (var bank in banks)
                {
                    Banks.Add(bank);
                }
                IsBanksLoaded = true;
            }
            catch (Exception ex)
            {
                IsBanksLoadFailed = true;
                await Shell.Current.DisplayAlert("Warning", $"Failed to load banks: {ex.Message}", "OK");
            }
        }

        private async Task SelectBankAsync(Bank bank)
        {
            if (bank != null)
            {
                await Shell.Current.GoToAsync(nameof(AuthPage), new Dictionary<string, object>
                {
                    { "Bank", bank }
                });
            }
        }
    }
}