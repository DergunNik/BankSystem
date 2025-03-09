using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BankSystem.Infrastructure.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Incoming request: {Path} {Method}", context.Request.Path, context.Request.Method);
            await _next(context);
        }
    }
}