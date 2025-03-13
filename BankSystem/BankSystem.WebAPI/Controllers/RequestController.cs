using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace BankSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/requests")]
    public class RequestController : ExtendedControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IUserService _userService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly ISalaryService _salaryService;
        private readonly ILogger<RequestController> _logger;

        public class RequestAnswerDto
        {
            public int RequestId { get; set; }
            public bool IsApproved { get; set; }
        }

        public RequestController(
            IRequestService requestService,
            IUserService userService,
            ILogger<RequestController> logger,
            IEnterpriseService enterpriseService,
            ISalaryService salaryService)
        {
            _requestService = requestService;
            _userService = userService;
            _logger = logger;
            _enterpriseService = enterpriseService;
            _salaryService = salaryService;
        }

        [HttpGet("salary-projects")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Request>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Request>>>
            GetSalaryProjectRequestsAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"salary-projects\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var requests = await _requestService.GetRequestsAsync(RequestType.SalaryProject, user.BankId);

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

        [HttpGet("salaries")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Request>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Request>>>
            GetSalaryRequestsAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"salaries\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var requests = await _requestService.GetRequestsAsync(RequestType.Salary, user.BankId);

                if (requests == null || !requests.Any())
                {
                    return NoContent();
                }

                return Ok(requests);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching salary requests");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("users")]
        [Authorize(Roles = "Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Request>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Request>>>
            GetUserRequestsAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"users\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var requests = await _requestService.GetRequestsAsync(RequestType.User, user.BankId);

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
        [ProducesResponseType(typeof(IReadOnlyCollection<Request>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Request>>>
            GetCreditRequestsAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"credits\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var requests = await _requestService.GetRequestsAsync(RequestType.Credit, user.BankId);

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

        [HttpPost("salary-projects")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult>
            AnswerSalaryProjectRequestAsync([FromBody] RequestAnswerDto answer)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"salary-projects\") {answer.RequestId} {answer.IsApproved}");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var request = await _requestService.GetRequestByIdAsync(answer.RequestId);

                if (request is null) return NoContent();
                if (request.IsChecked) return Conflict("Request has already been processed.");
                if (request.RequestType != RequestType.SalaryProject) return BadRequest();
                if (request.BankId != user.BankId) return Conflict("Request is from other bank");

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

        [HttpPost("salaries")]
        [Authorize(Roles = "ExternalSpecialist,Operator,Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult>
            AnswerSalaryRequestAsync([FromBody] RequestAnswerDto answer)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"salaries\") {answer.RequestId} {answer.IsApproved}");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var request = await _requestService.GetRequestByIdAsync(answer.RequestId);

                if (request is null) return NoContent();
                if (request.IsChecked) return Conflict("Request has already been processed.");
                if (request.RequestType != RequestType.Salary) return BadRequest();
                if (request.BankId != user.BankId) return Conflict("Request is from other bank");

                if (user.UserRole == UserRole.ExternalSpecialist)
                {
                    var userEnterprise = await _enterpriseService.GetExternalSpecialistEnterpriseAsync(userId);
                    if (userEnterprise is null) return BadRequest("Not enterprise worker");
                    var salary = await _salaryService.GetSalaryAsync(request.RequestEntityId);
                    if (salary is null) return BadRequest("Request error");
                    var project = await _salaryService.GetSalaryProjectAsync(salary.SalaryProjectId);
                    if (project is null) return BadRequest("Request error");
                    if (project.EnterpriseId != userEnterprise.Id)
                    {
                        return Conflict("Salary of other enterprise");
                    }
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
                _logger.LogError(e, "Error answering salary request");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("users")]
        [Authorize(Roles = "Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AnswerUserRequestAsync([FromBody] RequestAnswerDto answer)
        {
            return await ProcessRequestAsync(answer, RequestType.User);
        }

        [HttpPost("credits")]
        [Authorize(Roles = "Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AnswerCreditRequestAsync([FromBody] RequestAnswerDto answer)
        {
            return await ProcessRequestAsync(answer, RequestType.Credit);
        }

        private async Task<ActionResult> ProcessRequestAsync(RequestAnswerDto answer, RequestType expectedType)
        {
            try
            {
                _logger.LogInformation($"Processing {expectedType} request: {answer.RequestId} Approval: {answer.IsApproved}");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var request = await _requestService.GetRequestByIdAsync(answer.RequestId);

                if (request is null) return NoContent();
                if (request.IsChecked) return Conflict("Request has already been processed.");
                if (request.RequestType != expectedType) return BadRequest($"Expected {expectedType} request.");
                if (request.BankId != user.BankId) return Conflict("Request is from another bank.");

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
                _logger.LogError(e, $"Error processing {expectedType} request");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}