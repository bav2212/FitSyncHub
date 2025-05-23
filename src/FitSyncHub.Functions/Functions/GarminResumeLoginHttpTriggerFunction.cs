using FitSyncHub.GarminConnect.Auth.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class GarminResumeLoginHttpTriggerFunction
{
    private readonly IGarminAuthService _garminAuthService;

    public GarminResumeLoginHttpTriggerFunction(IGarminAuthService garminAuthService)
    {
        _garminAuthService = garminAuthService;
    }

    [Function(nameof(GarminResumeLoginHttpTriggerFunction))]
    public async Task<ActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "garmin/resume-login")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? mfa = req.Query["mfa"];
        if (string.IsNullOrWhiteSpace(mfa))
        {
            return new BadRequestObjectResult("mfa has wrong format");
        }

        await _garminAuthService.ResumeLogin(mfa, cancellationToken);
        return new OkObjectResult("Sucess");
    }
}
