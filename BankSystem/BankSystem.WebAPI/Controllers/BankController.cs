using BankSystem.BankClient.Abstractions.ServiceInterfaces;
using BankSystem.BankClient.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.WebAPI.Controllers
{
    [Route("api/banks")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly IBankService _bankService;
        private readonly ILogger<BankController> _logger;

        public BankController(IBankService bankService, ILogger<BankController> logger)
        {
            _bankService = bankService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<Bank>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IReadOnlyList<Bank>> GetBanks()
        {
            try
            {
                _logger.LogInformation("GetBanks");
                var banks = _bankService.GetAllBanksAsync().Result;
                return Ok(banks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetBanks");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
