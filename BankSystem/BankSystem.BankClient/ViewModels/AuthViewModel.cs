using BankSystem.BankClient.Enums;
using BankSystem.BankClient.Models;
using BankSystem.BankClient.Models.DTO;
using BankSystem.BankClient.Pages;
using BankSystem.BankClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BankSystem.BankClient.ViewModels
{
    [QueryProperty(nameof(Bank), "Bank")]
    public partial class AuthViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ITokenStorageService _tokenStorageService;

        public AuthViewModel(IAuthService authService, ITokenStorageService tokenStorageService)
        {
            _authService = authService;
            _tokenStorageService = tokenStorageService;
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            SelectRegisterCommand = new RelayCommand(SelectRegister);
            SelectLoginCommand = new RelayCommand(SelectLogin);
            LoadTitleCommand = new RelayCommand(LoadTitle);
            IsLog = false;
            IsReg = false;
            IsSelection = true;
            Title = "Authorization";

            UserRoles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Where(role => role != UserRole.BannedOrNotExisting)
                            .ToList();
        }

        [ObservableProperty] private bool isSelection;
        [ObservableProperty] private bool isLog;
        [ObservableProperty] private bool isReg;
        [ObservableProperty] private string title;

        [ObservableProperty] private Bank bank;
        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private string fullName;
        [ObservableProperty] private string passportSeriesAndNumber;
        [ObservableProperty] private string identificationNumber;
        [ObservableProperty] private string phone;
        [ObservableProperty] private UserRole selectedUserRole;
        [ObservableProperty] private List<UserRole> userRoles;

        public IAsyncRelayCommand RegisterCommand { get; }
        public IAsyncRelayCommand LoginCommand { get; }
        public RelayCommand LoadTitleCommand { get; }
        public RelayCommand SelectRegisterCommand { get; }
        public RelayCommand SelectLoginCommand { get; }

        private void LoadTitle()
        {
            Title = Bank?.LegalName ?? "AuthPage";
        }

        private void SelectRegister()
        {
            LoadTitle();
            IsSelection = false;
            IsReg = true;
            IsLog = false;
        }

        private void SelectLogin()
        {
            LoadTitle();
            IsSelection = false;
            IsReg = false;
            IsLog = true;
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(PassportSeriesAndNumber) ||
                string.IsNullOrWhiteSpace(IdentificationNumber) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                Bank == null)
            {
                await Shell.Current.DisplayAlert("Warning", "All fields are required for registration.", "OK");
                return;
            }

            try
            {
                var user = new User
                {
                    FullName = FullName,
                    PassportSeriesAndNumber = PassportSeriesAndNumber,
                    IdentificationNumber = IdentificationNumber,
                    Phone = Phone,
                    Email = Email,
                    PasswordHash = Password,
                    BankId = Bank.Id,
                    UserRole = SelectedUserRole
                };
                await _authService.RegisterAsync(user);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Warning", ex.Message, "OK");
            }

            await Shell.Current.DisplayAlert("Information", "Manager will check your request. Please wait.", "OK");
            await Shell.Current.GoToAsync(nameof(BanksPage));
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || Bank == null)
            {
                await Shell.Current.DisplayAlert("Warning", "Email, Password, and Bank are required for login.", "OK");
                return;
            }

            try
            {
                var loginRequest = new LoginRequestDto
                {
                    Email = Email,
                    Password = Password,
                    BankId = Bank.Id
                };

                var token = await _authService.LoginAsync(loginRequest);
                await _tokenStorageService.SaveTokenAsync(token);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Warning", ex.Message, "OK");
                return;
            }

            var role = await _tokenStorageService.GetRoleAsync();

            if (role == UserRole.BannedOrNotExisting.ToString())
            {
                await Shell.Current.DisplayAlert("Warning", "User is banned or not existing.", "OK");
                return;
            }
            else if (role == UserRole.Client.ToString())
            {
                //todo
            }
            else if (role == UserRole.Operator.ToString())
            {
                //todo
            }
            else if (role == UserRole.Manager.ToString())
            {
                //todo
            }
            else if (role == UserRole.ExternalSpecialist.ToString())
            {
                //todo
            }
            else if (role == UserRole.Administrator.ToString())
            {
                //todo
            }
        }
    }
}