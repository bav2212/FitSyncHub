using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Middlewares;

public sealed class LogBadRequestMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<LogBadRequestMiddleware> _logger;

    public LogBadRequestMiddleware(ILogger<LogBadRequestMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        await next(context);

        if (context.GetInvocationResult().Value is not ObjectResult result)
        {
            return;
        }

        if (result.StatusCode >= 400)
        {
            _logger.LogWarning(
                "Function {FunctionName} returned error {StatusCode}: {Message}",
                context.FunctionDefinition.Name,
                result.StatusCode,
                result.Value);
        }
    }
}
