using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Models
{
    public class BankReserve : BankRelatedEntity
    {
        public decimal Balance { get; set; } = 10_000_000m;
    }
}
