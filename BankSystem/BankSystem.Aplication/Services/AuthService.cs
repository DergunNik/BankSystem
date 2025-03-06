using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using BankSystem.Domain.Abstractions;
using Isopoh.Cryptography.Argon2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using BankSystem.Domain.Abstractions.ServiceInterfaces;

namespace BankSystem.Aplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IRequestService _requestService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IRequestService requestService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _requestService = requestService;
            _logger = logger;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            _logger.LogInformation($"LoginAsync {email} {password}");
            var user = await _unitOfWork.GetRepository<User>().ListAsync(user => user.Email == email);
            if (user.Count != 0 && Argon2.Verify(user[0].PasswordHash, password))
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
            else
            {
                throw new Exception("Invalid email or password");
            }
        }

        public async Task RegisterAsync(User user)
        {
            _logger.LogInformation($"RegisterAsync {user.ToString()}");
            var repository = _unitOfWork.GetRepository<User>();
            var thisEmailUserList = await repository.ListAsync(user => user.Email == user.Email);

            bool isEmailUsed = false;
            foreach(var u in thisEmailUserList)
            {
                if (u.IsApproved)
                {
                    isEmailUsed = true;
                    break;
                }
            }

            if (isEmailUsed)
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
