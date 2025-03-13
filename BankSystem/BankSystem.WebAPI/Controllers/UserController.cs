using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ExtendedControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("bank")]
        [Authorize(Roles = "Operator,Manager,Administrator")]
        [ProducesResponseType(typeof(IReadOnlyList<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<User>>> GetUsersByBank()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"bank\")");

                int userId;
                try
                {
                    userId = GetUserId();
                }
                catch (Exception)
                {
                    return BadRequest("Invalid user ID.");
                }

                var user = await _userService.GetUserAsync(userId);
                if (user == null)
                {
                    return BadRequest("Invalid user ID.");
                }

                var users = await _userService.GetUsersByBankIdAsync(user.BankId);
                return Ok(users);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving users by bank");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
