﻿using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/transfers")]
    [ApiController]
    public class TransferController : ExtendedControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITransferService _transferService;
        private readonly ILogger<TransferController> _logger;

        public class TransferDto
        {
            public int SourceAccountId { get; set; }
            public int DestinationAccountId { get; set; }
            public decimal Amount { get; set; }
        }

        public TransferController(
            IUserService userService,
            ITransferService transferService,
            ILogger<TransferController> logger)
        {
            _userService = userService;
            _transferService = transferService;
            _logger = logger;
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

        [HttpGet("from-bank")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Transfer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<Transfer>>>
            GetTransfersFromBankAsync()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"from-bank\")");

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

        [HttpGet("my-transfers")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Transfer>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetMyTransfersAsync()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"my-transfers\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

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