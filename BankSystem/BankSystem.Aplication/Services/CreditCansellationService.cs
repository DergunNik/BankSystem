using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class CreditCansellationService : ICreditCansellationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBankReserveService _bankReserveService;
        private readonly ILogger<CreditCansellationService> _logger;

        public CreditCansellationService(
            IUnitOfWork unitOfWork, 
            IBankReserveService bankReserveService, 
            ILogger<CreditCansellationService> logger)
        {
            _unitOfWork = unitOfWork;
            _bankReserveService = bankReserveService;
            _logger = logger;
        }

        public async Task CanselCreditAsync(int creditId)
        {
            _logger.LogInformation($"CanselCreditAsync {creditId}");
            var credit = await _unitOfWork.GetRepository<Credit>().GetByIdAsync(creditId) 
                ?? throw new Exception($"Invalid credit id {creditId}");

            if (credit.IsCanselled) throw new Exception($"Can't cansel canselled credit {creditId}");
            if (credit.PaidAmount > 0m) throw new Exception($"Can't cansel credit {creditId} with payments done");
            if (credit.AnswerDate < credit.RequestDate)
            {
                throw new Exception($"Credit {creditId} is not approved");
            }

            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(credit.AccountId) 
                ?? throw new Exception($"Invalid account id {credit.AccountId}");

            if (account.Balance < credit.CreditAmount)
            {
                throw new Exception("Account does not have enough money");
            }

            await _bankReserveService.TransferFromAccountBankAsync(account.Id, credit.CreditAmount);
            
            credit.IsCanselled = true;
            var cansel = new Cansel()
            {
                CansellationType = CansellationType.Credit,
                CanselledEntityId = credit.Id,
                CansellationDate = DateTime.UtcNow
            };

            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.GetRepository<Credit>().UpdateAsync(credit);
                await _unitOfWork.GetRepository<Cansel>().AddAsync(cansel);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"CanselCreditAsync {creditId}");
                await _unitOfWork.RollbackTransactionAsync();
                await _bankReserveService.TransferFromAccountBankAsync(account.Id, credit.CreditAmount);
            }
        }
    }
}
