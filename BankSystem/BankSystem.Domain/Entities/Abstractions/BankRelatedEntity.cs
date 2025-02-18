using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities.Abstactions
{
    public abstract class BankRelatedEntity : Entity
    {
        public int BankId { get; set; }
        public Bank Bank { get; set; }
    }
}
