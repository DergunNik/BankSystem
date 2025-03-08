using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
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
    public class AccountService : IAccountService
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

            if (account.IsSavingAccount && account.SavingsAccountUntil < DateTime.UtcNow) throw new Exception("Invalid date");
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

        public async Task DeleteAccountAsync(int accountId)
        {
            _logger.LogInformation($"DeleteAccountAsync {accountId}");
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            
            if (account is null) throw new Exception($"Account with {accountId} doesn't exist");
            if (account.IsFrozen || account.IsBlocked ||
                account.IsSavingAccount && account.SavingsAccountUntil >= DateTime.UtcNow)
            {
                throw new Exception("Account can't be deleted");
            }
            if (account.Balance != 0m) throw new Exception("Account balance is not 0");
        }

        public async Task<Account?> GetAccountAsync(int accountId)
        {
            _logger.LogInformation($"GetAccountAsync {accountId}");
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            return account;
        }

        public async Task ApplyMonthlyInterestAsync(int accountId)
        {
            _logger.LogInformation($"ApplyMonthlyInterestAsync {accountId}");
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            if (account is null) throw new Exception($"Account with {accountId} doesn't exist");

            if (account.CreationDate <= DateTime.UtcNow.AddMonths(-1))
            {
                var amount = account.Balance * account.MonthlyInterestRate / 100m;
                await _bankReserveService.TransferFromAccountBankAsync(accountId, amount);
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

        public async Task<(IReadOnlyCollection<Transfer>, IReadOnlyCollection<BankTransfer>)> GetAccountTransfersAsync(int accountId)
        {
            _logger.LogInformation($"GetAccountTransfers {accountId}");
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId) 
                ?? throw new Exception($"Invalid account id {accountId}");
            var transfers = await _unitOfWork.GetRepository<Transfer>()
                .ListAsync(a => a.SourceAccountId == accountId || a.DestinationAccountId == accountId);
            var bankTransfers = await _unitOfWork.GetRepository<BankTransfer>()
                .ListAsync(a => a.AccountId == accountId);
            return (transfers, bankTransfers);
        }

        public async Task<IReadOnlyCollection<Account>> GetAccountFromBankAsync(int bankId)
        {
            _logger.LogInformation($"GetAccountFromBankAsync {bankId}");
            var res = await _unitOfWork.GetRepository<Account>().ListAsync(a => a.BankId == bankId);
            return res.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyCollection<Account>> GetUserAccountsAsync(int userId)
        {
            _logger.LogInformation($"GetUsersAccountAsync {userId}");
            var res = await _unitOfWork.GetRepository<Account>()
                .ListAsync(a => a.OwnerType == AccountOwnerType.IndividualUser && a.OwnerId == userId);
            return res.ToList().AsReadOnly();
        }

        public bool CanWithdrawFrom(Account account)
        {
            return !account.IsFrozen && !account.IsBlocked && !account.IsSavingAccount;
        }

        public bool CanDepositTo(Account account)
        {
            return !account.IsBlocked && !account.IsSavingAccount;
        }
    }
}
