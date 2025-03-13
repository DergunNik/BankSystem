using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Application.Settings
{
    public class AuthSettings
    {
        public TimeSpan ExpirationTime { get; set; }
        public string SecretKey { get; set; }
    }
}
