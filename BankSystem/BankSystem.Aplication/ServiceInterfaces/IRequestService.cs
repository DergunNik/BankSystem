using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    public interface IRequestService
    {
        void CreateRequest(IRequestable requestTarget);
        void ApproveRequest(IRequestable requestTarget);
        void RejectRequest(IRequestable requestTarget);
        IReadOnlyList<IRequestable> GetAllRequests();
        IReadOnlyList<IRequestable> GetRelevantRequests(UserRole recepient);
    }
}
