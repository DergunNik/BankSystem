using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class User : BankRelatedEntity, IRequestable
    {
        public string FullName { get; set; }
        public string PassportSeriesAndNumber { get; set; }
        public string IdentificationNumber { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public UserRole UserRole { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime AnswerDate { get; set; }
        public string PasswordHash { get; set; }
    }
}
