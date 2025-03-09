using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class BankReserveService : IBankReserveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BankReserveService> _logger;

        public BankReserveService(IUnitOfWork unitOfWork, ILogger<BankReserveService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task GetMoneyFromStateBankAsync(int bankReserveId, decimal amount)
        {
            _logger.LogInformation($"GetMoneyFromStateBank {bankReserveId.ToString()}");
            if (amount <= 0m) return;

            try
            {
                _unitOfWork.BeginTransaction();
                var reserve = await _unitOfWork.GetRepository<BankReserve>().GetByIdAsync(bankReserveId);
                if (reserve is not null)
                {
                    reserve.Balance += amount;
                    _logger.LogWarning($"GetMoneyFromStateBank {bankReserveId.ToString()} {amount.ToString()}");
                    await _unitOfWork.GetRepository<BankReserve>().UpdateAsync(reserve);
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
            }
        }

        public async Task TransferFromBankAsync(int accountId, int bankReserveId, decimal amount, Entity? entity = null)
        {
            _logger.LogInformation($"TransferMoneyFromBank {accountId.ToString()} {bankReserveId.ToString()}");
            var (account, reserve) = await DoTransferPreparationAsync(accountId, bankReserveId, amount);
            if (reserve!.Balance < amount)
            {
                await GetMoneyFromStateBankAsync(bankReserveId, amount * 2);
            }

            await TransferAsync(account, reserve, amount, false, entity);
        }

        public async Task TransferFromAccountBankAsync(int accountId, decimal amount, Entity? entity = null)
        {
            _logger.LogInformation($"TransferFromAccountBankAsync {accountId.ToString()}");
            await TransferFromBankAsync(accountId, GetBankReserveAsync(accountId).Result.Id, amount, entity);
        }

        public async Task TransferToBankAsync(int accountId, int bankReserveId, decimal amount, Entity? entity = null)
        {
            _logger.LogInformation($"TransferMoneyToBank {accountId.ToString()} {bankReserveId.ToString()}");
            var (account, reserve) = await DoTransferPreparationAsync(accountId, bankReserveId, amount);
            if (account!.Balance < amount)
            {
                throw new Exception("Owner doesnt have enough money");
            }

            await TransferAsync(account, reserve, amount, true, entity);
        }

        public async Task TransferToAccountBankAsync(int accountId, decimal amount, Entity? entity = null)
        {
            _logger.LogInformation($"TransferToAccountBankAsync {accountId.ToString()}");
            await TransferToBankAsync(accountId, GetBankReserveAsync(accountId).Result.Id, amount, entity);
        }

        private async Task<(Account, BankReserve)> DoTransferPreparationAsync(int accountId, int bankReserveId, decimal amount)
        {
            if (amount < 0)
            {
                throw new Exception($"Invalid amount {amount.ToString()}");
            }
            
            var (account, reserve) = (await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId),
                                      await _unitOfWork.GetRepository<BankReserve>().GetByIdAsync(bankReserveId));
            
            if (account is null || reserve is null)
            {
                throw new Exception("Invalid transfer input");
            }
            return (account, reserve);
        }

        private async Task<BankReserve> GetBankReserveAsync(int accountId)
        {
            var account = await _unitOfWork.GetRepository<Account>().GetByIdAsync(accountId);
            if (account is null) throw new Exception($"Invalid account id {accountId} input");
            var reserve = await _unitOfWork.GetRepository<BankReserve>()
                .FirstOrDefaultAsync(r => r.BankId == account.BankId);
            if (reserve is null) throw new Exception($"Invalid bank id {account.BankId} input");
            return reserve;
        }

        private async Task TransferAsync(Account account, BankReserve reserve, decimal amount, bool toBank, Entity? entity = null)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var accountRepository = _unitOfWork.GetRepository<Account>();
                var reserveRepository = _unitOfWork.GetRepository<BankReserve>();
                if (toBank)
                {
                    account.Balance -= amount;
                    reserve.Balance += amount;
                }
                else
                {
                    reserve.Balance -= amount;
                    account.Balance += amount;
                }
                await accountRepository.UpdateAsync(account);
                await reserveRepository.UpdateAsync(reserve);

                if (entity is not null)
                {
                    var entityType = entity.GetType();
                    var repositoryType = typeof(IRepository<>).MakeGenericType(entityType);
                    var repository = _unitOfWork.GetType().GetMethod("GetRepository")?
                        .MakeGenericMethod(entityType)
                        .Invoke(_unitOfWork, null);
                    if (repository is not null)
                    {
                        var updateMethod = repositoryType.GetMethod("UpdateAsync");
                        if (updateMethod is not null)
                        {
                            await (Task)updateMethod.Invoke(repository, new object[] { entity });
                        }
                    }
                }

                var bankTransfer = new BankTransfer()
                {
                    AccountId = account.Id,
                    BankReserveId = reserve.Id,
                    Amount = amount,
                    TransferDate = DateTime.UtcNow,
                    IsIncomingToBank = toBank,
                };
                await _unitOfWork.GetRepository<BankTransfer>().AddAsync(bankTransfer);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
            }
        }

        public async Task<BankTransfer?> GetBankTransferAsync(int id)
        {
            return await _unitOfWork.GetRepository<BankTransfer>().GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<BankTransfer>> GetUserBankTransfersAsync(int userId)
        {
            var userAccounts = await _unitOfWork.GetRepository<Account>()
                    .ListAsync(a => a.OwnerType == AccountOwnerType.IndividualUser && a.OwnerId == userId);
            List<BankTransfer> ret = [];
            foreach (var account in userAccounts)
            {
                var accountTransfers = await _unitOfWork.GetRepository<BankTransfer>()
                    .ListAsync(t => t.AccountId == account.Id);
                foreach (var transfer in accountTransfers)
                {
                    ret.Add(transfer);
                }
            }
            return ret.AsReadOnly();
        }
    }
}
