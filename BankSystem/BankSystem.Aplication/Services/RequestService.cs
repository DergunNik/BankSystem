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
    public class RequestService : IRequestService
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ILogger<RequestService> _logger;

        public RequestService(IUnitOfWork unitOfWork, ILogger<RequestService> logger) 
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ApproveRequestAsync(IRequestable requestTarget)
        {
            _logger.LogInformation($"ApproveRequestAsync {requestTarget.ToString()}");
            await HandleRequestAsync(true, requestTarget);
        }

        public async Task RejectRequestAsync(IRequestable requestTarget)
        {
            _logger.LogInformation($"RejectRequestAsync {requestTarget.ToString()}");
            await HandleRequestAsync(false, requestTarget);
        }
        public async Task ApproveRequestAsync(Request request)
        {
            await ApproveRequestAsync(await GetRequestEntityAsync(request));
        }

        public async Task RejectRequestAsync(Request request)
        {
            await RejectRequestAsync(await GetRequestEntityAsync(request));
        }

        public async Task CreateRequestAsync(IRequestable requestTarget)
        {
            _logger.LogInformation($"CreateRequestAsync {requestTarget.ToString()}");
            try
            {
                _unitOfWork.BeginTransaction();

                var (repositoryType, typedRepository) = GetRepository(requestTarget);

                var addMethod = repositoryType.GetMethod("AddAsync");
                if (addMethod != null)
                {
                    requestTarget.RequestDate = DateTime.UtcNow;
                    await (Task)addMethod.Invoke(typedRepository, new object[] { requestTarget });
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }   
        }

        public async Task<IReadOnlyCollection<Request>> GetRequecstsAsync(RequestType requestType)
        {
            _logger.LogInformation($"GetRequecstsAsync {requestType.ToString()}");
            var res = await _unitOfWork.GetRepository<Request>().ListAsync(r => r.RequestType == requestType);
            return res.ToList().AsReadOnly();
        }

        public async Task<IRequestable> GetRequestEntityAsync(Request request)
        {
            _logger.LogInformation($"GetRequestEntity {request.ToString()}");
            dynamic? entityRepository = null;
            RequestType? requestType = null;

            switch (request.RequestType)
            {
                case RequestType.User:
                    entityRepository = _unitOfWork.GetRepository<User>();
                    break;
                case RequestType.Credit:
                    entityRepository = _unitOfWork.GetRepository<Credit>();
                    break;
                default:
                    throw new Exception("Not supported type is requestable");
            }

            return await entityRepository.GetByIdAsync(request.SenderId);
        }

        private async Task HandleRequestAsync(bool isApproved, IRequestable requestTarget)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                dynamic? entityRepository = null;
                var requestRepository = _unitOfWork.GetRepository<Request>();
                RequestType? requestType = null;

                if (requestTarget is User)
                {
                    entityRepository = _unitOfWork.GetRepository<User>();
                    requestType = RequestType.User;
                }
                else if (requestTarget is Credit)
                {
                    entityRepository = _unitOfWork.GetRepository<Credit>();
                    requestType = RequestType.Credit;
                }
                else
                {
                    throw new Exception("Not supported type is requestable");
                }

                if (isApproved)
                {
                    requestTarget.IsApproved = true;
                    requestTarget.AnswerDate = DateTime.UtcNow;
                    await entityRepository?.UpdateAsync(requestTarget);
                }
                else
                {
                    await entityRepository?.DeleteAsync(requestTarget);
                }
                var request = await requestRepository.FirstOrDefaultAsync(
                        r => r.RequestType == requestType && r.RequestEntityId == requestTarget.Id);
                if (request is not null)
                {
                    request.IsChecked = true;
                    await requestRepository.UpdateAsync(request);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private (Type, dynamic?) GetRepository(IRequestable requestTarget)
        {
            Type targetType = requestTarget.GetType();
            var repositoryType = typeof(IRepository<>).MakeGenericType(targetType);
            var repository = _unitOfWork.GetType()
                .GetMethod("GetRepository")
                .MakeGenericMethod(targetType)
                .Invoke(_unitOfWork, null);
            dynamic typedRepository = Convert.ChangeType(repository, repositoryType);
            return (repositoryType, typedRepository);
        }
    }
}
