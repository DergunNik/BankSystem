﻿using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface IRequestService
    {
        Task CreateRequestAsync(IRequestable requestTarget);
        Task ApproveRequestAsync(Request request);
        Task RejectRequestAsync(Request request);
        Task<IReadOnlyCollection<Request>> GetRequestsAsync(RequestType requestType, int bankId);
        Task<IRequestable> GetRequestEntityAsync(Request request);
        Task<Request?> GetRequestByIdAsync(int id);
    }
}
