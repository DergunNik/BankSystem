using BankSystem.BankClient.Models;
using BankSystem.BankClient.Models.DTO;
using BankSystem.BankClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.ViewModels
{
    [QueryProperty(nameof(Bank), "Bank")]
    public partial class AuthViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        public AuthViewModel(IAuthService authService)
        {
            _authService = authService;
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        [ObservableProperty] private Bank bank;
        [ObservableProperty] private string email;
        [ObservableProperty] private string password;

        public IAsyncRelayCommand RegisterCommand { get; }
        public IAsyncRelayCommand LoginCommand { get; }

        private async Task RegisterAsync()
        {
            var user = new User
            {
                Email = Email,
                PasswordHash = Password,
                BankId = Bank.Id
            };
            await _authService.RegisterAsync(user);
        }

        private async Task LoginAsync()
        {
            var loginRequest = new LoginRequestDto
            {
                Email = Email,
                Password = Password,
                BankId = Bank.Id
            };
            await _authService.LoginAsync(loginRequest);
        }
    }
}
