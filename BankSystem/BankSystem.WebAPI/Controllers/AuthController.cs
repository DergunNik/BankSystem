using BankSystem.Aplication.ServiceInterfaces;
using BankSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("authorization")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] User user)
        {
            try
            {
                await authService.RegisterAsync(user);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status409Conflict, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] string email, [FromBody] string password)
        {
            try
            {
                var token = await authService.LoginAsync(email, password);
                return Ok(token);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, e.Message);
            }
        }
    }
}
