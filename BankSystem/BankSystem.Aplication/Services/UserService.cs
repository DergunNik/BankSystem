using BankSystem.BankClient.Abstractions;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            return await _unitOfWork.GetRepository<User>().GetByIdAsync(userId);
        }

        public async Task<IReadOnlyCollection<User>> GetUsersByBankIdAsync(int bankId)
        {
            return await _unitOfWork.GetRepository<User>().ListAsync(u => u.BankId == bankId);
        }
    }
}
