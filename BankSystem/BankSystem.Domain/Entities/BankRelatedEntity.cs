using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public abstract class BankRelatedEntity : Entity
    {
        public int BankId { get; set; }
    }
}
