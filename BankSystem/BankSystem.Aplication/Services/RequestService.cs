using BankSystem.Domain.Abstractions;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
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
        readonly ICreditService _creditService;
        readonly ILogger<RequestService> _logger;

        public RequestService(IUnitOfWork unitOfWork, ICreditService creditService, ILogger<RequestService> logger) 
        {
            _creditService = creditService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ApproveRequestAsync(Request request)
        {
            _logger.LogInformation($"ApproveRequestAsync {request.ToString()}");
            await ApproveRequestAsync(await GetRequestEntityAsync(request), request);
        }

        public async Task RejectRequestAsync(Request request)
        {
            _logger.LogInformation($"RejectRequestAsync {request.ToString()}");
            await RejectRequestAsync(await GetRequestEntityAsync(request), request);
        }

        public async Task CreateRequestAsync(IRequestable requestTarget)
        {
            _logger.LogInformation($"CreateRequestAsync {requestTarget.ToString()}");

            try
            {
                _unitOfWork.BeginTransaction();
                var (repositoryType, typedRepository) = GetRepository(requestTarget);

                var request = new Request()
                {
                    RequestEntityId = requestTarget.Id,
                    IsChecked = false
                };

                if (requestTarget is Credit)
                {
                    request.RequestType = RequestType.Credit;
                } else if (requestTarget is User)
                {
                    request.RequestType = RequestType.User;
                }
                else if (requestTarget is SalaryProject)
                {
                    request.RequestType = RequestType.SalaryProject;
                } else
                {
                    throw new Exception("Invalid type");
                }

                var addMethod = repositoryType.GetMethod("AddAsync");
                if (addMethod != null)
                {
                    requestTarget.RequestDate = DateTime.UtcNow;
                    requestTarget.AnswerDate = requestTarget.RequestDate.AddMinutes(-1);
                    await (Task)addMethod.Invoke(typedRepository, new object[] { requestTarget });
                }

                await _unitOfWork.GetRepository<Request>().AddAsync(request);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }   
        }

        public async Task<IReadOnlyCollection<Request>> GetRequestsAsync(RequestType requestType)
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
                case RequestType.SalaryProject:
                    entityRepository = _unitOfWork.GetRepository<SalaryProject>();
                    break;
                default:
                    throw new Exception("Not supported type is requestable");
            }

            return await entityRepository.GetByIdAsync(request.SenderId);
        }

        public async Task<Request?> GetRequestByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<Request>().GetByIdAsync(id);
        }

        private async Task ApproveRequestAsync(IRequestable requestTarget, Request request)
        {
            _logger.LogInformation($"ApproveRequestAsync {requestTarget.ToString()}");
            await HandleRequestAsync(true, requestTarget, request);
        }

        private async Task RejectRequestAsync(IRequestable requestTarget, Request request)
        {
            _logger.LogInformation($"RejectRequestAsync {requestTarget.ToString()}");
            await HandleRequestAsync(false, requestTarget, request);
        }

        private async Task HandleRequestAsync(bool isApproved, IRequestable requestTarget, Request request)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                dynamic? entityRepository = null;
                var requestRepository = _unitOfWork.GetRepository<Request>();
                RequestType? requestType = null;

                if (requestTarget is User user)
                {
                    entityRepository = _unitOfWork.GetRepository<User>();
                    requestType = RequestType.User;
                    bool isEmailUsed = false;
                    foreach (var u in _unitOfWork.GetRepository<User>()
                        .ListAsync(u => u.Email == user.Email).Result)
                    {
                        if (u.IsApproved)
                        {
                            isEmailUsed = true;
                            break;
                        }
                    }
                    isApproved = isApproved && !isEmailUsed;
                }
                else if (requestTarget is Credit credit)
                {
                    entityRepository = _unitOfWork.GetRepository<Credit>();
                    requestType = RequestType.Credit;
                    isApproved = isApproved && _unitOfWork.GetRepository<User>().GetByIdAsync(credit.UserId).Result is not null
                                            && _unitOfWork.GetRepository<Account>().GetByIdAsync(credit.AccountId).Result is not null;
                }
                else if (requestTarget is SalaryProject project)
                {
                    entityRepository = _unitOfWork.GetRepository<Credit>();                    
                    requestType = RequestType.SalaryProject;
                    var enterprise = await _unitOfWork.GetRepository<Enterprise>()
                        .GetByIdAsync(project.EnterpriseId);
                    isApproved = isApproved && enterprise is not null
                                            && await _unitOfWork.GetRepository<Account>()
                                                .FirstOrDefaultAsync(a => a.OwnerId == enterprise.Id 
                                                && a.OwnerType == AccountOwnerType.Enterprise)
                                                is not null;
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
                    if (requestType == RequestType.Credit)
                    {
                        await _creditService.InitCreditAsync(((Credit)requestTarget).Id);
                    }
                }
                
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
