using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Services
{
    public class TokenStorageService : ITokenStorageService
    {
        private const string TokenKey = "jwt_token";
        private const string RoleKey = "user_role";

        public Task SaveTokenAsync(string token)
        {
            SecureStorage.SetAsync(TokenKey, token);
            return Task.CompletedTask;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.GetAsync(TokenKey);
        }

        public Task RemoveTokenAsync()
        {
            SecureStorage.Remove(TokenKey);
            return Task.CompletedTask;
        }

        public Task SaveRoleAsync(string role)
        {
            SecureStorage.SetAsync(RoleKey, role);
            return Task.CompletedTask;
        }

        public async Task<string> GetRoleAsync()
        {
            return await SecureStorage.GetAsync(RoleKey);
        }
    }
}
