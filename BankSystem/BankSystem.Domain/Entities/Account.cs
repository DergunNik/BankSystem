using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Account : BankRelatedEntity
    {
        public decimal Balance { get; set; }
        public int OwnerId { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsFrozen{ get; set; }
        public bool IsSavingsAccount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public DateTime UnavailableUntil { get; set; }
        public AccountOwnerType OwnerType { get; set; }
    }
}
