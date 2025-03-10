using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Models
{
    public abstract class BankRelatedEntity : Entity
    {
        public int BankId { get; set; }
    }
}
