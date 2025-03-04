using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountService> _logger;
        private readonly IBankReserveService _bankReserveService;

        public AccountService(IUnitOfWork unitOfWork, ILogger<AccountService> logger, IBankReserveService bankReserveService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _bankReserveService = bankReserveService;
        }

        public async Task CreateAccountAsync(Account account)
        {
            _logger.LogInformation($"CreateAccountAsync {account.ToString()}");
            switch (account.OwnerType)
            {
                case AccountOwnerType.IndividualUser:
                    if (_unitOfWork.GetRepository<User>()
                        .FirstOrDefaultAsync(u => u.Id == account.OwnerId).Result is null)
                    {
                        throw new Exception($"No account owner with selected Id exist");
                    }
                    break;
                case AccountOwnerType.Enterprise:
                    if (_unitOfWork.GetRepository<Enterprise>()
                        .FirstOrDefaultAsync(u => u.Id == account.OwnerId).Result is null)
                    {
                        throw new Exception($"No enterprise with selected Id exist");
                    }
                    break;
                default:
                    throw new Exception("Not supported type is owner");
            }

            if (account.UnavailableUntil < DateTime.UtcNow) throw new Exception("Invalid date");
            if (account.MonthlyInterestRate < 0) throw new Exception("Invalid interes rate");
            if (_unitOfWork.GetRepository<Bank>().GetByIdAsync(account.Id).Result is null)
            { 
                throw new Exception("Invalid bank id");
            }

            account.Balance = 0m;
            account.IsFrozen = false;
            account.IsBlocked = false;
            account.CreationDate = DateTime.UtcNow;

            _unitOfWork.BeginTransaction();
            await _unitOfWork.GetRepository<Account>().AddAsync(account);
            await _unitOfWork.CommitTransactionAsync();
         
            _logger.LogInformation($"CreateAccountAsync {account.ToString()} successful");
        }

        public async Task<Account?> GetAccountAsync(int accountId)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            _logger.LogInformation($"GetAccountAsync {account?.ToString()}");
            return account;
        }

        public async Task ApplyMonthlyInterestAsync(int accountId)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            _logger.LogInformation($"ApplyMonthlyInterestAsync {account?.ToString()}");
            if (account is null) throw new Exception($"Account with {accountId} doesn't exist");

            if (account.CreationDate <= DateTime.UtcNow.AddMonths(-1))
            {
                var reserve = await _unitOfWork.GetRepository<BankReserve>()
                    .FirstOrDefaultAsync(r => r.BankId == account.BankId);
                if (reserve is null) throw new Exception("Invalid account's bank id");

                var amount = account.Balance * account.MonthlyInterestRate / 100m;
                await _bankReserveService.TransferMoneyFromBankAsync(accountId, reserve.Id, amount);
            }
        }

        public async Task BlockAccountAsync(int accountId)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            _logger.LogInformation($"BlockAccountAsync {account?.ToString()}");
            if (account is null) throw new Exception($"Account with {accountId} doesn't exist");
            account.IsBlocked = true;
        }

        public async Task FreezeAccountAsync(int accountId)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            _logger.LogInformation($"FreezeAccountAsync {account?.ToString()}");
            if (account is null) throw new Exception($"Account with {accountId} doesn't exist");
            account.IsFrozen = true;
        }

        public async Task UnblockAccountAsync(int accountId)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            _logger.LogInformation($"UnblockAccountAsync {account?.ToString()}");
            if (account is null) throw new Exception($"Account with {accountId} doesn't exist");
            account.IsBlocked = false;
        }

        public async Task UnfreezeAccountAsync(int accountId)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            _logger.LogInformation($"UnfreezeAccountAsync {account?.ToString()}");
            if (account is null) throw new Exception($"Account with {accountId} doesn't exist");
            account.IsFrozen = false;
        }
    }
}
