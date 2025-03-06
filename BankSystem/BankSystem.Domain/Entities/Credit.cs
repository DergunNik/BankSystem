using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Credit : BankRelatedEntity, IRequestable, ICansellable
    {
        public decimal CreditAmount {get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCanselled { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime AnswerDate { get; set; }
        public int DurationInMonths { get; set; }
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public string Reason { get; set; }
        public decimal InterestRate { get; set; }
    }
}
