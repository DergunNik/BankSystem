using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Transfer : BankRelatedEntity
    {
        public AccountOwnerType SenderType { get; set; }
        public AccountOwnerType RecepientType{ get; set; }
        public int SenderId { get; set; }
        public int RecepientId { get; set; }
        public decimal Amount { get; set; }
    }
}
