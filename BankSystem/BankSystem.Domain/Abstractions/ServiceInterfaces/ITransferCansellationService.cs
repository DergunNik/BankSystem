﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface ITransferCansellationService
    {
        Task CanselTransferAsync(int transferId);
    }
}
