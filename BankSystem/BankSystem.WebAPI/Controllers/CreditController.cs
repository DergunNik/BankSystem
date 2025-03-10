using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using BankSystem.BankClient.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static BankSystem.WebAPI.Controllers.AccountController;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/credits")]
    [ApiController]
    public class CreditController : ExtendedControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICreditService _creditService;
        private readonly IRequestService _requestService;
        private readonly IAccountService _accountService;
        private readonly ILogger<CreditController> _logger;

        public class CreditDto
        {
            public decimal CreditAmount { get; set; }
            public int DurationInMonths { get; set; }
            public int AccountId { get; set; }
            public decimal InterestRate { get; set; }
            public string Reason { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Credit>> GetCreditAsync([FromBody] CreditDto creditDto)
        {
            try
            {
                _logger.LogInformation("HttpPost");
                if (creditDto.InterestRate < 0m || creditDto.CreditAmount <= 0m
                    || creditDto.DurationInMonths <= 0 || String.IsNullOrEmpty(creditDto.Reason))
                {
                    return Conflict("Bad args.");
                }

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var account = await _accountService.GetAccountAsync(creditDto.AccountId);
                if (account is null) return BadRequest("Invalid account ID.");

                if (account.OwnerType != AccountOwnerType.IndividualUser ||
                    account.OwnerId != userId)
                {
                    return BadRequest("Invalid account ID.");
                }

                if (account.IsBlocked || account.IsFrozen || account.IsSavingAccount)
                {
                    return BadRequest("This account is unacceptable for getting credit.");
                }

                var credit = new Credit
                {
                    CreditAmount = creditDto.CreditAmount,
                    TotalAmount = creditDto.CreditAmount * creditDto.InterestRate,
                    PaidAmount = 0,
                    IsPaid = false,
                    DurationInMonths = creditDto.DurationInMonths,
                    UserId = userId,
                    AccountId = creditDto.AccountId,
                    Reason = creditDto.Reason,
                    InterestRate = creditDto.InterestRate
                };

                await _requestService.CreateRequestAsync(credit);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "HttpPost");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(Credit), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Credit>> GetCreditAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{id:int}\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var credit = await _creditService.GetCreditAsync(id);
                if (credit is null) return BadRequest("Invalid credit ID.");

                if (credit.BankId != user.BankId) return Conflict("Other bank.");

                return Ok(credit);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpGet(\"{id}\")");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("of-bank")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyList<Credit>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<Credit>>> GetCreditFromBankAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var credits = await _creditService.GetCreditsOfBankAsync(user.BankId);

                return Ok(credits);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "HttpGet");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("my-credits")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(IReadOnlyList<Credit>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<Credit>>> GetCreditAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var credits = await _creditService.GetCreditsByUserIdAsync(userId);

                return Ok(credits);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "HttpGet");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
