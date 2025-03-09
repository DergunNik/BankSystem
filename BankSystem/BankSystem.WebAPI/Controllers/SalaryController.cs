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
    public class SalaryController : ExtendedControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISalaryService _salaryService;
        private readonly IRequestService _requestService;
        private readonly IAccountService _accountService;
        private readonly IEnterpriseService _enterpriseService;
        private readonly ILogger<SalaryController> _logger;

        public class SalaryDto
        {
            public decimal Amount { get; set; }
            public int UserAccountId { get; set; }
            public int SalaryProjectId { get; set; }
        }

        public SalaryController(
            IUserService userService,
            ISalaryService salaryService,
            ILogger<SalaryController> logger,
            IRequestService requestService,
            IAccountService accountService,
            IEnterpriseService enterpriseService)
        {
            _userService = userService;
            _salaryService = salaryService;
            _logger = logger;
            _requestService = requestService;
            _accountService = accountService;
            _enterpriseService = enterpriseService;
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(Salary), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Salary>> GetSalaryAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{id}\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var salary = await _salaryService.GetSalaryAsync(id);
                if (salary is null) return NotFound();
                if (user.BankId != salary.BankId) return Conflict("From other bank");

                return Ok(salary);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpGet(\"{id}\")");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("from-bank")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Salary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetSalariesFromBankAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"from-bank\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var accounts = await _salaryService.GetSalariesFromBankAsync(user.BankId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("of-enterprise/{enterpriseId:int}")]
        [Authorize(Roles = "ExternalSpecialist,Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Salary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetSalariesOfEnterpriseAsync(int enterpriseId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"of-enterprise/{enterpriseId}\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var enterprise = await _enterpriseService.GetEnterpriseAsync(enterpriseId);
                if (enterprise is null) return BadRequest();

                if (enterprise.BankId != user.BankId) return Conflict("From other bank");

                if (user.UserRole == UserRole.ExternalSpecialist)
                {
                    var userEnterprise = await _enterpriseService.GetExternalSpecialistEnterpriseAsync(userId);
                    if (userEnterprise is null) return BadRequest("Not enterprise worker");
                    if (enterpriseId != userEnterprise.Id) return Conflict("Salaries of other enterprise");
                }

                var accounts = await _salaryService.GetEnterpriseSalariesAsync(enterpriseId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting salaries");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("of-user/{userId:int}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Salary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetSalariesOfUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"of-user/{userId}\")");

                int adminId;
                try { adminId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var admin = await _userService.GetUserAsync(adminId);
                if (admin is null) return BadRequest("Invalid user ID.");

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (admin.BankId != user.BankId) return Conflict("Other bank");

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
        [ProducesResponseType(typeof(IReadOnlyCollection<Salary>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Salary>>> GetMySalariesAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"my-salaries\")");
                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (account.OwnerId != userId) return BadRequest();
                if (project.BankId != user.BankId
                    || account.BankId != user.BankId) return Conflict("Other bank");

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

        [HttpDelete("remove/{id:int}")]
        [Authorize(Roles = "Client,ExternalSpecialist,Operator,Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveSalaryAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpDelete(\"remove/{id}\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var salary = await _salaryService.GetSalaryAsync(id);
                if (salary is null) return BadRequest("Invalid salary ID.");

                if (user.UserRole == UserRole.Client)
                {
                    var account = await _accountService.GetAccountAsync(salary.UserAccountId);
                    if (account is null) return BadRequest("Invalid account ID.");

                    if (account.OwnerType != AccountOwnerType.IndividualUser
                        && account.OwnerId != userId)
                    {
                        return Conflict("Not person's salary");
                    }
                }
                else if (user.UserRole == UserRole.ExternalSpecialist)
                {
                    var project = await _salaryService.GetSalaryProjectAsync(salary.SalaryProjectId);
                    var userEnterprise = await _enterpriseService.GetExternalSpecialistEnterpriseAsync(userId);
                    if (project is null || userEnterprise is null) return BadRequest("Wrong id");
                    if (userEnterprise.Id != project.EnterpriseId) return Conflict("Other enterprise");
                }

                await _salaryService.RemoveSalaryAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpDelete(\"remove/{id}\")");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpDelete("remove/of-enterprise/{enterpriseId:int}")]
        [Authorize(Roles = "ExternalSpecialist,Operator,Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveEnterpriseSalariesAsync(int enterpriseId)
        {
            try
            {
                _logger.LogInformation($"HttpDelete(\"remove/of-enterprise/{enterpriseId}\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (user.UserRole == UserRole.ExternalSpecialist)
                {
                    var userEnterprise = await _enterpriseService.GetExternalSpecialistEnterpriseAsync(userId);
                    if (userEnterprise is null) return BadRequest("Wrong id");
                    if (userEnterprise.Id != enterpriseId) return Conflict("Other enterprise");
                }

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