using BankSystem.Aplication.Services;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace BankSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/requests")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly ILogger<RequestController> _logger;

        public class RequestAnswerDto
        {
            public int RequestId { get; set; }
            public bool IsApproved { get; set; }
        }

        public RequestController(IRequestService requestService, ILogger<RequestController> logger)
        {
            _requestService = requestService;
            _logger = logger;
        }

        // gets

        [HttpGet("salary-projects")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult<IReadOnlyCollection<Request>>>
            GetSalaryProjectRequestsAsync()
        {
            _logger.LogInformation($"HttpGet(\"salary-projects\")");
            try
            {
                var requests = await _requestService.GetRequestsAsync(RequestType.SalaryProject);

                if (requests == null || !requests.Any())
                {
                    return NoContent(); 
                }

                return Ok(requests);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching salary project requests");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("users")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<ActionResult<IReadOnlyCollection<Request>>>
            GetUserRequestsAsync()
        {
            _logger.LogInformation($"HttpGet(\"users\")");
            try
            {
                var requests = await _requestService.GetRequestsAsync(RequestType.User);

                if (requests == null || !requests.Any())
                {
                    return NoContent();
                }

                return Ok(requests);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching user requests");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("credits")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<ActionResult<IReadOnlyCollection<Request>>>
            GetCreditRequestsAsync()
        {
            _logger.LogInformation($"HttpGet(\"credits\")");
            try
            {
                var requests = await _requestService.GetRequestsAsync(RequestType.Credit);

                if (requests == null || !requests.Any())
                {
                    return NoContent();
                }

                return Ok(requests);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching credit requests");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        // posts

        [HttpPost("salary-projects")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult>
            AnswerSalaryProjectRequestAsync([FromBody] RequestAnswerDto answer)
        {
            _logger.LogInformation($"HttpPost(\"salary-projects\") {answer.RequestId} {answer.IsApproved}");
            try
            {
                var request = await _requestService.GetRequestByIdAsync(answer.RequestId);
                
                if (request is null) return NoContent();
                if (request.IsChecked) return Conflict("Request has already been processed.");
                if (request.RequestType != RequestType.SalaryProject) return BadRequest();

                if (answer.IsApproved)
                {
                    await _requestService.ApproveRequestAsync(request);
                }
                else
                {
                    await _requestService.RejectRequestAsync(request);
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error answering salary project request");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("users-or-credits")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<ActionResult>
            AnswerUserOrCreditRequestAsync([FromBody] RequestAnswerDto answer)
        {
            _logger.LogInformation($"HttpPost(\"users-or-credits\") {answer.RequestId} {answer.IsApproved}");
            try
            {
                var request = await _requestService.GetRequestByIdAsync(answer.RequestId);

                if (request is null) return NoContent();
                if (request.IsChecked) return Conflict("Request has already been processed.");
                if (request.RequestType != RequestType.User
                    && request.RequestType != RequestType.Credit)
                {
                    return BadRequest();
                }

                if (answer.IsApproved)
                {
                    await _requestService.ApproveRequestAsync(request);
                }
                else
                {
                    await _requestService.RejectRequestAsync(request);
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error answering user or credit request");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
