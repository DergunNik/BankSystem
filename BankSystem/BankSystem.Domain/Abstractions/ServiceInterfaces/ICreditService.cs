﻿using BankSystem.BankClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.BankClient.Abstractions.ServiceInterfaces
{
    public interface ICreditService
    {
        Task InitCreditAsync(int creditId);
        Task PayCreditAsync(int creditId, int accountId, decimal amount);
        Task HandleTodaysCreditPaymentsAsync();
        Task<Credit?> GetCreditAsync(int id);
        Task<IReadOnlyList<Credit>> GetCreditsByUserIdAsync(int userId);
        Task<IReadOnlyList<Credit>> GetCreditsByUserIdAsync(int userId, bool isPaid);
        Task<IReadOnlyList<Credit>> GetCreditsOfBankAsync(int bankId);
    }
}
