using BankSystem.Aplication.Services;
using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/transfer")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly ILogger<TransferController> _logger;

        public class TransferDto
        {
            public int SourceAccountId { get; set; }
            public int DestinationAccountId { get; set; }
            public decimal Amount { get; set; }
        }

        public TransferController(ITransferService transferService, ILogger<TransferController> logger)
        {
            _transferService = transferService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult>
            TransferAsync([FromBody] TransferDto transferDto)
        {
            _logger.LogInformation("HttpPost");
            try
            {
                await _transferService.TransferAsync(
                    transferDto.SourceAccountId,
                    transferDto.DestinationAccountId,
                    transferDto.Amount);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error transfering");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("from-bank/{bankId}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult<IReadOnlyCollection<Transfer>>>
            GetTransfersFromBankAsync(int bankId)
        {
            _logger.LogInformation($"HttpGet(\"from-bank/{bankId}\")");
            try
            {
                return Ok(await _transferService.GetTransferFromBank(bankId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting transfers");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{transferId}")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        public async Task<ActionResult<Transfer>>
            GetTransferAsync(int transferId)
        {
            _logger.LogInformation($"HttpGet(\"{transferId}\")");
            try
            {
                var transfer = await _transferService.GetTransferAsync(transferId);
                if (transfer is null) return NotFound();
                return Ok(transfer);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting transfers");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("my-transfers")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult> GetMyTransfersAsync()
        {
            _logger.LogInformation("HttpGet(\"my-transfers\")");
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }
                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid user ID.");
                }

                var accounts = await _transferService.GetUserTransfersAsync(userId);
                return Ok(accounts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting user transfers");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
