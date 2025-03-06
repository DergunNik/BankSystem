using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Cansel : BankRelatedEntity
    {
        public CansellationType CansellationType { get; set; }
        public int CanselledEntityId { get; set; }
        public DateTime Date { get; set; }
    }
}
