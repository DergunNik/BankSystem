using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Services
{
    public interface ITokenStorageService
    {
        Task SaveTokenAsync(string token);
        Task<string?> GetTokenAsync();
        Task RemoveTokenAsync();
        Task SaveRoleAsync(string role);
        Task<string> GetRoleAsync();
    }
}
