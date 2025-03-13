using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static BankSystem.WebAPI.Controllers.TransferController;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/transfers")]
    [ApiController]
    public class TransferController : ExtendedControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITransferService _transferService;
        private readonly IBankReserveService _bankReserveService;
        private readonly ILogger<TransferController> _logger;

        public class TransferDto
        {
            public int SourceAccountId { get; set; }
            public int DestinationAccountId { get; set; }
            public decimal Amount { get; set; }
        }

        public class UserTransfers
        {
            public IReadOnlyList<Transfer> UsersTransfers { get; set; }
            public IReadOnlyList<BankTransfer> BankTransfers { get; set; }
        }

        public TransferController(
            IUserService userService,
            ITransferService transferService,
            ILogger<TransferController> logger,
            IBankReserveService bankReserveService)
        {
            _userService = userService;
            _transferService = transferService;
            _logger = logger;
            _bankReserveService = bankReserveService;
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult>
            TransferAsync([FromBody] TransferDto transferDto)
        {
            try
            {
                _logger.LogInformation("HttpPost");
                await _transferService.TransferAsync(
                    transferDto.SourceAccountId,
                    transferDto.DestinationAccountId,
                    transferDto.Amount);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error transferring");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("bank")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Transfer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Transfer>>>
            GetTransfersFromBankAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"bank\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                return Ok(await _transferService.GetTransferFromBank(user.BankId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting transfers");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{transferId}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(Transfer), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Transfer>>
            GetTransferAsync(int transferId)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{transferId}\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var transfer = await _transferService.GetTransferAsync(transferId);
                if (transfer is null) return NotFound();

                if (transfer.BankId != user.BankId) return Conflict("Other bank");

                return Ok(transfer);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting transfers");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Transfer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTransfers>> GetMyTransfersAsync(int userId)
        {
            try
            {
                _logger.LogInformation("HttpGet(\"user\")");

                int adminId;
                try { adminId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var admin = await _userService.GetUserAsync(adminId);
                if (admin is null) return BadRequest("Invalid user ID.");

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                if (admin.BankId != user.BankId) return Conflict("other bank.");

                var userTransfers = new UserTransfers();
                userTransfers.UsersTransfers = await _transferService.GetUserTransfersAsync(userId);
                userTransfers.BankTransfers = await _bankReserveService.GetUserBankTransfersAsync(userId);
                return Ok(userTransfers);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting user transfers");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        } 

        [HttpGet("my")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Transfer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserTransfers>> GetMyTransfersAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"my\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var userTransfers = new UserTransfers();
                userTransfers.UsersTransfers = await _transferService.GetUserTransfersAsync(userId);
                userTransfers.BankTransfers = await _bankReserveService.GetUserBankTransfersAsync(userId);
                return Ok(userTransfers);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting user transfers");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}