using BankSystem.BankClient.Models;
using BankSystem.BankClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJwtService _jwtService;
        private readonly ITokenStorageService _tokenStorageService;

        public AuthService(HttpClient httpClient, IJwtService jwtService, ITokenStorageService tokenStorageService)
        {
            _httpClient = httpClient;
            _jwtService = jwtService;
            _tokenStorageService = tokenStorageService;
        }

        public async Task<string> RegisterAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", user);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> LoginAsync(LoginRequestDto loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();

            await _tokenStorageService.SaveTokenAsync(token);   

            var claims = _jwtService.DecodeToken(token);
            if (claims.TryGetValue("role", out var role))
            {
                await _tokenStorageService.SaveRoleAsync(role);
            }

            return token;
        }
    }
}
