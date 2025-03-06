using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem.Aplication.Services
{
    public class TransferCansellationService : ITransferCansellationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransferService _transferService;
        private readonly ILogger<TransferCansellationService> _logger;

        public TransferCansellationService(
            IUnitOfWork unitOfWork, 
            ILogger<TransferCansellationService> logger,
            ITransferService transferService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _transferService = transferService;
        }

        public async Task CanselTransferAsync(int transferId)
        {
            _logger.LogInformation($"CanselTransferAsync {transferId}");
            var transfer = await _unitOfWork.GetRepository<Transfer>().GetByIdAsync(transferId);
            if (transfer is null)
            {
                throw new Exception($"Invalid transfer id {transferId}");
            }
            if (transfer.IsCanselled) throw new Exception($"Can't cansel canselled transfer {transferId}");

            await _transferService.TransferAsync(
                    transfer.DestinationAccountId,
                    transfer.SourceAccountId,
                    transfer.Amount);

            transfer.IsCanselled = true;
            var cansel = new Cansel()
            {
                CansellationType = CansellationType.Transfer,
                CanselledEntityId = transferId,
                CansellationDate = DateTime.UtcNow
            };

            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.GetRepository<Transfer>().UpdateAsync(transfer);
                await _unitOfWork.GetRepository<Cansel>().AddAsync(cansel);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"CanselTransferAsync {transferId}");
                await _unitOfWork.RollbackTransactionAsync();
                await _transferService.TransferAsync(
                        transfer.SourceAccountId,
                        transfer.DestinationAccountId,
                        transfer.Amount);
            }
        }
    }
}
