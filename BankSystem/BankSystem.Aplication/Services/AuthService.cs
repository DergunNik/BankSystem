using BankSystem.Aplication.ServiceInterfaces;
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

namespace BankSystem.Aplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IRequestService _requestService;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IRequestService requestService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _requestService = requestService;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
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
            var repository = _unitOfWork.GetRepository<User>();
            var thisEmailUserList = await repository.ListAsync(user => user.Email == user.Email);
            if (thisEmailUserList.ToList<User>().Count == 0)
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
