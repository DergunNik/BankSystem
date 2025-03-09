using BankSystem.Domain.Abstractions.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/authorization")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public class LoginRequestDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public int BankId { get; set; }
        }

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        public async Task<ActionResult> RegisterAsync([FromBody] User user)
        {
            try
            {
                _logger.LogInformation($"HttpPost(\"register\") {user.ToString()}");
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
        public async Task<ActionResult> LoginAsync(
            [FromBody] LoginRequestDto args)
        {
            var email = args.Email;
            var password = args.Password;
            var bankId = args.BankId;
            try
            {
                _logger.LogInformation($"HttpPost(\"login\") {email} {bankId}");
                var token = await _authService.LoginAsync(email, password, bankId);
                return Ok(token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"HttpPost(\"login\") {email} {bankId}");
                return StatusCode(StatusCodes.Status401Unauthorized, e.Message);
            }
        }
    }
}