using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface IUserService
    {
        Task<User?> GetUserAsync(int userId);
        Task<IReadOnlyCollection<User>> GetUsersAsync(int bankId);
    }
}
