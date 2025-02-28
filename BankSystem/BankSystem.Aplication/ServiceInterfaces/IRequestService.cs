using BankSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.ServiceInterfaces
{
    public interface IRequestService
    {
        void CreateRequest(User sender, IRequestable requestTarget);
        void ApproveRequest(User recepient, IRequestable requestTarget);
        void RejectRequest(User recepient, IRequestable requestTarget);
        IReadOnlyList<IRequestable> GetAllRequests();
        IReadOnlyList<IRequestable> GetRelevantRequests(User recepient);
    }
}
