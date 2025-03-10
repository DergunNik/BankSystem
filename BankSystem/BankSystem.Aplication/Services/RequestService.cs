using BankSystem.BankClient.Abstractions;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
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

                var request = new Request()
                {
                    RequestEntityId = requestTarget.Id,
                    IsChecked = false,
                    BankId = requestTarget.BankId
                };

                switch (requestTarget)
                {
                    case Credit credit:
                        request.RequestType = RequestType.Credit;
                        await _unitOfWork.GetRepository<Credit>().AddAsync(credit);
                        break;
                    case User user:
                        request.RequestType = RequestType.User;
                        await _unitOfWork.GetRepository<User>().AddAsync(user);
                        break;
                    case SalaryProject project:
                        request.RequestType = RequestType.SalaryProject;
                        await _unitOfWork.GetRepository<SalaryProject>().AddAsync(project);
                        break;
                    case Salary salary:
                        request.RequestType = RequestType.Salary;
                        await _unitOfWork.GetRepository<Salary>().AddAsync(salary);
                        break;
                    default:
                        throw new Exception("Invalid type");
                }

                requestTarget.RequestDate = DateTime.UtcNow;
                requestTarget.AnswerDate = DateTime.UtcNow.AddMinutes(-1);

                await _unitOfWork.GetRepository<Request>().AddAsync(request);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating request");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IReadOnlyCollection<Request>> GetRequestsAsync(RequestType requestType, int bankId)
        {
            _logger.LogInformation($"GetRequecstsAsync {requestType.ToString()}");
            var res = await _unitOfWork.GetRepository<Request>()
                .ListAsync(r => r.RequestType == requestType && r.BankId == bankId);
            return res.ToList().AsReadOnly();
        }

        public async Task<IRequestable> GetRequestEntityAsync(Request request)
        {
            _logger.LogInformation($"GetRequestEntity {request.ToString()}");
            dynamic? entityRepository = null;

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
                case RequestType.Salary:
                    entityRepository = _unitOfWork.GetRepository<Salary>();
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
                else if (requestTarget is Salary salary)
                {
                    entityRepository = _unitOfWork.GetRepository<Salary>();
                    requestType = RequestType.Salary;
                    var salaryAccount = await _unitOfWork.GetRepository<Account>().GetByIdAsync(salary.UserAccountId);
                    var salaryProject = await _unitOfWork.GetRepository<SalaryProject>().GetByIdAsync(salary.SalaryProjectId);
                    isApproved = isApproved && salaryAccount is not null
                                            && salaryProject is not null
                                            && salary.Amount > 0m;
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
    }
}
