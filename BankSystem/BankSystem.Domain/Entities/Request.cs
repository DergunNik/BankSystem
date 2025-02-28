using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities
{
    public class Request : BankRelatedEntity
    {
        public int SenderId { get; set; }
        public RequestType requestType { get; set; }
        public int RequestEntityId { get; set; }
    }
}
