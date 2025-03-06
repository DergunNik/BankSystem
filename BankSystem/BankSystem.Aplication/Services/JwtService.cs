using BankSystem.Aplication.Settings;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class JwtService : IJwtService
    {
        private readonly IOptions<AuthSettings> _options;
        private readonly ILogger<JwtService> _logger;
        public JwtService(IOptions<AuthSettings> options, ILogger<JwtService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            _logger.LogInformation($"GenerateToken {user.ToString()}");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.UserRole.ToString())
            };
            var jwtToken = new JwtSecurityToken(
                expires: DateTime.UtcNow.Add(_options.Value.ExpirationTime),
                claims: claims,
                signingCredentials: 
                    new SigningCredentials(
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_options.Value.SecretKey)),
                                SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}
