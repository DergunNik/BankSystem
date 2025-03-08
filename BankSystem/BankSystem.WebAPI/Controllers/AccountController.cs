using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using BankSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ExtendedControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public class AccountDto
        {
            public bool IsSavingAccount { get; set; }
            public decimal MonthlyInterestRate { get; set; }
            public DateTime SavingsAccountUntil { get; set; }
        }

        public AccountController(
            IRequestService requestService, 
            ILogger<AccountController> logger, 
            IUserService userService,
            IAccountService accountService)
        {
            _requestService = requestService;
            _logger = logger;
            _userService = userService;
            _accountService = accountService;
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult> AddAccountAsync([FromBody] AccountDto accountDto)
        {
            try
            {
                _logger.LogInformation("HttpPost");
                if (accountDto.IsSavingAccount && (accountDto.MonthlyInterestRate < 0 
                    || accountDto.SavingsAccountUntil < DateTime.UtcNow))
                {
                    return Conflict("Bad args.");
                }

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var account = new Account
                {
                    Balance = 0,
                    OwnerId = userId,
                    IsBlocked = false,
                    IsFrozen = false,
                    IsSavingAccount = accountDto.IsSavingAccount,
                    MonthlyInterestRate = accountDto.MonthlyInterestRate,
                    SavingsAccountUntil = accountDto.SavingsAccountUntil,
                    OwnerType = AccountOwnerType.IndividualUser,
                    BankId = user.BankId
                };

                await _requestService.CreateRequestAsync(user);
                
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating account");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{accountId}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult> GetAccountAsync(int accountId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{accountId}\"");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var account = await _accountService.GetAccountAsync(accountId);
                if (account is null)
                {
                    _logger.LogWarning($"Account with ID {accountId} not found.");
                    return NotFound("Account not found.");
                }

                if (user.BankId != account.BankId)
                {
                    return BadRequest("Account is from other bank.");
                }

                return Ok(account);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting account");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("from-bank")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult> GetAccountsAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"from-bank\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var accounts = await _accountService.GetAccountFromBankAsync(user.BankId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting accounts");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("my-accounts")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult> GetMyAccountsAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"my-accounts\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var accounts = await _accountService.GetUserAccountsAsync(userId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting users accounts");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
