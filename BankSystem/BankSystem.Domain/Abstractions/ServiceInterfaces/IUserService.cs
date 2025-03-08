using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Abstractions.ServiceInterfaces
{
    public interface IUserService
    {
        Task<User?> GetUserAsync(int userId);
        Task<IReadOnlyCollection<User>> GetUsersAsync(int bankId);
    }
}
