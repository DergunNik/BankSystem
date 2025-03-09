using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/cansel")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class CanselController : ExtendedControllerBase
    {
        private readonly ICreditCansellationService _creditCansellationService;
        private readonly ICreditService _creditService;
        private readonly ICanselRestorationService _canselRestorationService;
        private readonly ITransferCansellationService _transferCansellationService;
        private readonly ITransferService _transferService;
        private readonly IUserService _userService;
        private readonly ILogger<CanselController> _logger;

        public CanselController(
            ICreditCansellationService creditCansellationService,
            ICreditService creditService,
            ICanselRestorationService canselRestorationService,
            ITransferCansellationService transferCansellationService,
            ITransferService transferService,
            IUserService userService,
            ILogger<CanselController> logger)
        {
            _creditCansellationService = creditCansellationService;
            _creditService = creditService;
            _canselRestorationService = canselRestorationService;
            _transferCansellationService = transferCansellationService;
            _transferService = transferService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("credit/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CanselCreditAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"credit/{id}\")");

                var credit = await _creditService.GetCreditAsync(id);
                if (credit is null) return BadRequest("Invalid credit id");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (credit.BankId != user.BankId) return Conflict("Other bank");

                await _creditCansellationService.CanselCreditAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error cancelling credit");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("transfer/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CanselTransferAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"transfer/{id}\")");

                var transfer = await _transferService.GetTransferAsync(id);
                if (transfer is null) return BadRequest("Invalid transfer id");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (transfer.BankId != user.BankId) return Conflict("Other bank");

                await _transferCansellationService.CanselTransferAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error cancelling transfer");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("cansel/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreCanselAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"cansel/{id}\")");

                var cansel = await _canselRestorationService.GetCanselAsync(id);
                if (cansel is null) return BadRequest("Invalid credit id");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (cansel.BankId != user.BankId) return Conflict("Other bank");

                await _canselRestorationService.RestoreCansellationAsync(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error restoring cancellation");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("cansel/{id:int}")]
        [ProducesResponseType(typeof(Cansel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCanselAsync(int id)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"cansel/{id}\")");

                var cansel = await _canselRestorationService.GetCanselAsync(id);
                if (cansel is null) return BadRequest("Invalid credit id");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (cansel.BankId != user.BankId) return Conflict("Other bank");

                return Ok(cansel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting cancellation");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("cansel/from-bank")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Cansel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCanselFromBankAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"cansel/from-bank\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                return Ok(await _canselRestorationService.GetCanselsFromBankAsync(user.BankId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting cancellations from bank");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}