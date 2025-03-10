using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(User user); 
        Task<string> LoginAsync(string email, string password, int bankId);
    }
}
