using FitSyncHub.Strava.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions.Strava;

public sealed class StravaExchangeTokenHttpTriggerFunction
{
    private readonly StravaOAuthTokenService _stravaOAuthTokenService;

    public StravaExchangeTokenHttpTriggerFunction(
        StravaOAuthTokenService stravaOAuthTokenService)
    {
        _stravaOAuthTokenService = stravaOAuthTokenService;
    }

    [Function(nameof(StravaExchangeTokenHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "strava/exchange_token")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? code = req.Query["code"];
        string? scope = req.Query["scope"];

        if (scope == null)
        {
            return new BadRequestObjectResult("Invalid scope");
        }

        if (code is null)
        {
            return new BadRequestObjectResult("Code is required");
        }

        await _stravaOAuthTokenService.ExchangeTokenAsync(scope, code, cancellationToken);
        return new OkResult();
    }
}
