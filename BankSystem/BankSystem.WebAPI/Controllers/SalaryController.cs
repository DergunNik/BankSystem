using BankSystem.Aplication.Services;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/salaries")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly ISalaryService _salaryService;
        private readonly IRequestService _requestService;
        private readonly IAccountService _accountService;
        private readonly ILogger<SalaryController> _logger;

        public class SalaryDto
        {
            public decimal Amount { get; set; }
            public int UserAccountId { get; set; }
            public int SalaryProjectId { get; set; }
        }

        public SalaryController(
            ISalaryService salaryService, 
            ILogger<SalaryController> logger, 
            IRequestService requestService,
            IAccountService accountService)
        {
            _salaryService = salaryService;
            _logger = logger;
            _requestService = requestService;
            _accountService = accountService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult<Salary>> GetSalaryAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{id}\")");
                var salary = await _salaryService.GetSalaryAsync(id);
                if (salary is null) return NotFound();
                return Ok(salary);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpGet(\"{id}\")");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
  
        [HttpGet("from-bank/{bankId}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetSalariesFromBankAsync(int bankId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"from-bank/{bankId}\")");
                var accounts = await _salaryService.GetSalariesFromBankAsync(bankId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("of-enterprise/{enterpriseId}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetSalariesOfEnterpriseAsync(int enterpriseId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"of-enterprise/{enterpriseId}\")");
                var accounts = await _salaryService.GetEnterpriseSalariesAsync(enterpriseId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("of-user/{userId}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetSalariesOfUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"of-user/{userId}\")");
                var accounts = await _salaryService.GetUserSalariesAsync(userId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("my-salaries")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetMySalariesAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"my-salaries\")");
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }
                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid user ID.");
                }
                var accounts = await _salaryService.GetUserSalariesAsync(userId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult> AddSalaryAsync([FromBody] SalaryDto salaryDto)
        {
            try
            {
                _logger.LogInformation("HttpPost");
                var account = await _accountService.GetAccountAsync(salaryDto.UserAccountId);
                var project = await _salaryService.GetSalaryProjectAsync(salaryDto.SalaryProjectId);

                if (account is null || project is null
                    || account.OwnerType != AccountOwnerType.IndividualUser
                    || salaryDto.Amount <= 0)
                {
                    return BadRequest();
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }
                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid user ID.");
                }
                if (account.OwnerId != userId) return BadRequest();

                var salary = new Salary
                {
                    Amount = salaryDto.Amount,
                    UserAccountId = salaryDto.UserAccountId,
                    SalaryProjectId = salaryDto.SalaryProjectId
                };
                await _requestService.CreateRequestAsync(salary);

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpPost {salaryDto.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpDelete("remove/{id}")]
        [Authorize(Roles = "Client,ExternalSpecialist")]
        public async Task<ActionResult> RemoveSalaryAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpDelete(\"remove/{id}\")");
                await _salaryService.RemoveSalaryAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpDelete(\"remove/{id}\")");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpDelete("remove/of-enterprise/{enterpriseId}")]
        [Authorize(Roles = "ExternalSpecialist")]
        public async Task<ActionResult> RemoveEnterpriseSalariesAsync(int enterpriseId)
        {
            try
            {
                _logger.LogInformation($"HttpDelete(\"remove/of-enterprise/{enterpriseId}\")");
                var salaries = await _salaryService.GetEnterpriseSalariesAsync(enterpriseId);
                foreach (var salary in salaries)
                {
                    try
                    {
                        await _salaryService.RemoveSalaryAsync(salary.Id);
                    }
                    catch (Exception inner_e)
                    {
                        _logger.LogError(inner_e, $"HttpDelete(\"remove/of-enterprise/{enterpriseId}\") {salary.Id}");
                    }
                }
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpDelete(\"remove/of-enterprise/{enterpriseId}\")");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
 