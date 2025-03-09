using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/logs")]
    [Authorize(Roles = "Administrator")]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;
        private const string LogDirectory = "logs";
        private const string LogPattern = "banks-*.log";

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{count:int}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<string>> GetLogs(int count)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{count}\")");
                var logs = ReadLogs(count);
                if (logs == null)
                {
                    return NotFound("Log file is not found");
                }
                return Ok(logs);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetLogs {count}");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{count:int}/{level}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<string>> GetLogsByLevel(int count, string level)
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"{count}/{level}\")");
                var logs = ReadLogs(count, level);
                if (logs == null)
                {
                    return NotFound("Log file is not found");
                }
                return Ok(logs);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetLogs {count} {level}");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("last")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<string> GetLastLog()
        {
            try
            {
                _logger.LogInformation($"HttpGet(\"last\")");
                var logs = ReadLogs(1);
                if (logs != null && logs.Any())
                {
                    return Ok(logs.First());
                }
                return NotFound("Log file is not found");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetLogs last");
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        private List<string>? ReadLogs(int count, string? level = null)
        {
            var logFile = GetLatestLogFile();
            if (logFile == null) return null;

            var lines = System.IO.File.ReadLines(logFile).Reverse();

            if (!string.IsNullOrEmpty(level))
            {
                level = $"[{level.ToUpper()}]";
                lines = lines.Where(line => line.Contains(level));
            }

            return lines.Take(count).ToList();
        }

        private string? GetLatestLogFile()
        {
            return Directory.GetFiles(LogDirectory, LogPattern)
                .OrderByDescending(System.IO.File.GetLastWriteTime)
                .FirstOrDefault();
        }
    }
}
