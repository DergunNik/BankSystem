using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Bank : Entity
    {
        public string Address { get; set; }
        public string BIC { get; set; }
        public string LegalName { get; set; }
    }
}
