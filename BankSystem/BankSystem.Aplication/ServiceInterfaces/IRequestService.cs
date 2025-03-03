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
        Task CreateRequestAsync(IRequestable requestTarget);
        Task ApproveRequestAsync(IRequestable requestTarget);
        Task RejectRequestAsync(IRequestable requestTarget);
        Task<IReadOnlyCollection<Request>> GetRequecstsAsync(RequestType requestType);
        Task<IRequestable> GetRequestEntity(Request request);
    }
}
