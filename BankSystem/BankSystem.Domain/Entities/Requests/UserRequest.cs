using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Domain.Entities.Requests
{
    public class UserRequest : BankRelatedEntity, IRequest
    {
        public int SenderId { get; set; }
        public bool IsChecked { get; set; } = false;
        public int RequestEntityId { get; set; }
    }
}
