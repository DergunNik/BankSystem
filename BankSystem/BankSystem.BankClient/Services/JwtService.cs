using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Services
{
    public class JwtService : IJwtService
    {
        public IDictionary<string, string> DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken?.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
        }
    }
}
