using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(User user);
        Task<string> LoginAsync(string email, string password);
    }
}
