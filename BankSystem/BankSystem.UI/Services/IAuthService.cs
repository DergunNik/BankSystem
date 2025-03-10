using BankSystem.BankClient.Models;
using BankSystem.BankClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(User user);
        Task<string> LoginAsync(LoginRequestDto loginRequest);
    }
}
