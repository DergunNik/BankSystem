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

namespace BankSystem.Aplication.Services
{
    public class AuthService(IUnitOfWork unitOfWork, IJwtService jwtService) : IAuthService
    {
        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await unitOfWork.GetRepository<User>().ListWhere(nameof(User.Email), email);
            if (user.Count != 0 && Argon2.Verify(user[0].PasswordHash, password))
            {
                return jwtService.GenerateToken(user[0]);
            } 
            else
            {
                throw new Exception("Invalid email or password");
            }
        }

        public async Task RegisterAsync(User user)
        {
            var repository = unitOfWork.GetRepository<User>();
            var thisEmailUserList = await repository.ListWhere(nameof(User.Email), user.Email);
            if (thisEmailUserList.ToList<User>().Count == 0)
            {
                user.PasswordHash = Argon2.Hash(user.PasswordHash);
                await repository.AddAsync(user);
            }
            else
            {
                throw new Exception("User with this email already exists");
            }
        }
    }
}
