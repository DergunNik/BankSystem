using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Models
{
    public class ExternalSpecialist : BankRelatedEntity
    {
        public int EnterpriseId { get; set; }
        public int SpecielistId { get; set; }
    }
}
