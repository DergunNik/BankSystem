using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
using BankSystem.BankClient.Abstractions;
using Isopoh.Cryptography.Argon2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;

namespace BankSystem.Aplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IBankService _bankService;
        private readonly IRequestService _requestService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IBankService bankService, IRequestService requestService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _bankService = bankService;
            _requestService = requestService;
            _logger = logger;
        }

        public async Task<string> LoginAsync(string email, string password, int bankId)
        {
            _logger.LogInformation($"LoginAsync {email} {password} {bankId}");
            var user = await _unitOfWork.GetRepository<User>().ListAsync(u => u.Email == email && u.BankId == bankId);
            if (user.Count == 1 && Argon2.Verify(user[0].PasswordHash, password))
            {
                if (user[0].IsApproved)
                {
                    return _jwtService.GenerateToken(user[0]);
                } 
                else
                {
                    throw new Exception("This user is not approved");
                }
            }
            else if (user.Count > 1)
            {
                throw new Exception("Too many users with this email. Ask operator");
            }
            else
            {
                throw new Exception("Invalid email or password");
            }
        }

        public async Task RegisterAsync(User user)
        {
            _logger.LogInformation($"RegisterAsync {user.ToString()}");
            var repository = _unitOfWork.GetRepository<User>();

            if (String.IsNullOrEmpty(user.FullName) ||
                String.IsNullOrEmpty(user.PassportSeriesAndNumber) ||
                String.IsNullOrEmpty(user.IdentificationNumber) ||
                String.IsNullOrEmpty(user.Phone) ||
                String.IsNullOrEmpty(user.Email) ||
                String.IsNullOrEmpty(user.PasswordHash))
            {
                throw new Exception("Can't contain empty fields");
            }

            if (!_bankService.DoesBankWithIdExistAsync(user.BankId).Result)
            {
                throw new Exception($"Invalid bankId {user.BankId}");
            }

            var thisEmailUserList = await repository.ListAsync(u => u.Email == user.Email && u.BankId == user.BankId);

            bool isEmailUsed = false;
            foreach(var u in thisEmailUserList)
            {
                if (u.IsApproved)
                {
                    isEmailUsed = true;
                    break;
                }
            }

            if (!isEmailUsed)
            {
                user.PasswordHash = Argon2.Hash(user.PasswordHash);
                await _requestService.CreateRequestAsync(user);
            }
            else
            {
                throw new Exception("User with this email already exists");
            }
        }
    }
}
