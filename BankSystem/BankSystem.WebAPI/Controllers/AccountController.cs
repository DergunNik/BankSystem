using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpPost("block/{accountId:int}/{isBlocked:bool}")]
        [Authorize(Roles = "Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> HandleAccountBlockingAsync(int accountId, bool isBlocked)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"block/{accountId}/{isBlocked}\")");
                
                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var account = await _accountService.GetAccountAsync(accountId);
                if (account is null) return BadRequest("Invalid account ID.");

                if (account.BankId != user.BankId) return Conflict("Other bank");

                if (isBlocked)
                {
                    await _accountService.BlockAccountAsync(accountId);
                }
                else
                {
                    await _accountService.UnblockAccountAsync(accountId);
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error (un)blocking account");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("freeze/{accountId:int}/{isFreezed:bool}")]
        [Authorize(Roles = "Manager,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> HandleAccountFreezingAsync(int accountId, bool isFreezed)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"freeze/{accountId}/{isFreezed}\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var account = await _accountService.GetAccountAsync(accountId);
                if (account is null) return BadRequest("Invalid account ID.");

                if (account.BankId != user.BankId) return Conflict("Other bank");

                if (isFreezed)
                {
                    await _accountService.FreezeAccountAsync(accountId);
                }
                else
                {
                    await _accountService.UnfreezeAccountAsync(accountId);
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error (un)freezing account");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{accountId:int}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Account>> GetAccountAsync(int accountId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{accountId}\")");

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

        [HttpGet("bank")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(List<Account>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Account>>> GetAccountsAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"bank\")");

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

        [HttpGet("my")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(List<Account>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Account>>> GetMyAccountsAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"my\")");

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
