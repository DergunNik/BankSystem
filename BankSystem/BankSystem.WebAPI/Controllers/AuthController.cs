using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("authorization")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] User user)
        {
            _logger.LogInformation($"HttpPost(\"register\") {user.ToString()}");
            try
            {
                await _authService.RegisterAsync(user);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpPost(\"register\") {user.ToString()}");
                return StatusCode(StatusCodes.Status409Conflict, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(
            [FromBody] string email, 
            [FromBody] string password, 
            [FromBody] int bankId)
        {
            _logger.LogInformation($"HttpPost(\"register\") {email} {password} {bankId}");
            try
            {
                var token = await _authService.LoginAsync(email, password, bankId);
                return Ok(token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpPost(\"register\") {email} {password} {bankId}");
                return StatusCode(StatusCodes.Status401Unauthorized, e.Message);
            }
        }
    }
}
