using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class SalaryProject : BankRelatedEntity, IRequestable
    {
        public bool IsApproved { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime AnswerDate { get; set; }
        public DateTime SalaryDate { get; set; }
        public int EnterpriseId { get; set; }
        public int EnterpriseAccountId { get; set; }
        public string Details { get; set; }
    }
}
