using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Entities;
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

        public async Task TransferMoneyFromBankAsync(int accountId, int bankReserveId, decimal amount)
        {
            _logger.LogInformation($"TransferMoneyFromBank {accountId.ToString()} {bankReserveId.ToString()}");
            var (account, reserve) = await DoTransferPreparation(accountId, bankReserveId, amount);
            if (reserve!.Balance < amount)
            {
                await GetMoneyFromStateBankAsync(bankReserveId, amount * 2);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var accountRepository = _unitOfWork.GetRepository<Account>();
                var reserveRepository = _unitOfWork.GetRepository<BankReserve>();
                reserve!.Balance -= amount;
                account!.Balance += amount;
                await accountRepository.UpdateAsync(account);
                await reserveRepository.UpdateAsync(reserve);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
            }
        }

        public async Task TransferMoneyToBankAsyncx(int accountId, int bankReserveId, decimal amount)
        {
            _logger.LogInformation($"TransferMoneyToBank {accountId.ToString()} {bankReserveId.ToString()}");
            var (account, reserve) = await DoTransferPreparation(accountId, bankReserveId, amount);
            if (account!.Balance < amount)
            {
                throw new Exception("Owner doesnt have enough money");
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var accountRepository = _unitOfWork.GetRepository<Account>();
                var reserveRepository = _unitOfWork.GetRepository<BankReserve>();
                account!.Balance -= amount;
                reserve!.Balance += amount;
                await accountRepository.UpdateAsync(account);
                await reserveRepository.UpdateAsync(reserve);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
            }
        }

        private async Task<(Account?, BankReserve?)> DoTransferPreparation(int accountId, int bankReserveId, decimal amount)
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
    }
}
