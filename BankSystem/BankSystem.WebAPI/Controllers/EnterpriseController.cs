using BankSystem.Application.Services;
using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/enterprises")]
    [ApiController]
    public class EnterpriseController : ExtendedControllerBase
    {
        private readonly IEnterpriseService _enterpriseService;
        private readonly IUserService _userService;
        private readonly ILogger<BankController> _logger;

        public EnterpriseController(
            IEnterpriseService enterpriseService, 
            ILogger<BankController> logger, 
            IUserService userService)
        {
            _enterpriseService = enterpriseService;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("bank")]
        [Authorize]
        [ProducesResponseType(typeof(IReadOnlyList<Enterprise>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<Enterprise>>> GetEnterprises()
        {
            try
            {
                _logger.LogInformation("HttpGet(\"bank\")");

                int userId;
                try { userId = GetUserId(); }
                catch (Exception) { return BadRequest("Invalid user ID."); }

                var user = await _userService.GetUserAsync(userId);
                if (user is null) return BadRequest("Invalid user ID.");

                var banks = _enterpriseService.GetBankEnterprisesAsync(user.BankId).Result;
                return Ok(banks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "HttpGet(\"bank\")");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
