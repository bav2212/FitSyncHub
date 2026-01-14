using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftRacingSetAuthCookieHttpTriggerFunction
{
    private readonly ZwiftRacingService _zwiftRacingService;

    public ZwiftRacingSetAuthCookieHttpTriggerFunction(ZwiftRacingService zwiftRacingService)
    {
        _zwiftRacingService = zwiftRacingService;
    }

#if DEBUG
    [Function(nameof(ZwiftRacingSetAuthCookieHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-racing-set-cookie")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? cookie = req.Query["cookie"];
        if (string.IsNullOrWhiteSpace(cookie))
        {
            return new BadRequestObjectResult($"ZwiftRacing cookie is not specified, so will use only data from Zwift. Specify params: {nameof(cookie)}");
        }

        await _zwiftRacingService.SetCookie(cookie, cancellationToken);
        return new OkResult();
    }
}
