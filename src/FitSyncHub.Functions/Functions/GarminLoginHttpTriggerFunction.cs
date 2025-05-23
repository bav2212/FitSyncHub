using FitSyncHub.GarminConnect.Auth.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class GarminLoginHttpTriggerFunction
{
    private readonly IGarminAuthService _garminAuthService;

    public GarminLoginHttpTriggerFunction(IGarminAuthService garminAuthService)
    {
        _garminAuthService = garminAuthService;
    }

    [Function(nameof(GarminLoginHttpTriggerFunction))]
    public async Task<ActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "garmin/login")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var loginResult = await _garminAuthService.Login(cancellationToken);

        return loginResult.MfaRequired
            // do not change text here, it is used in apple login shortcut
            ? new OkObjectResult("MFA code required")
            : new OkObjectResult("Ok");
    }
}
