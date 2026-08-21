using FitSyncHub.Zwift.HttpClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftProfileInfoHttpTriggerFunction
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ILogger<ZwiftProfileInfoHttpTriggerFunction> _logger;

    public ZwiftProfileInfoHttpTriggerFunction(
        ZwiftHttpClient zwiftHttpClient,
        ILogger<ZwiftProfileInfoHttpTriggerFunction> logger)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _logger = logger;
    }

    [Function(nameof(ZwiftProfileInfoHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-profile-info")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var profileIdQueryParam = req.Query["profileId"];
        if (string.IsNullOrWhiteSpace(profileIdQueryParam))
        {
            return new BadRequestObjectResult("Specify params: profileId");
        }

        if (!long.TryParse(profileIdQueryParam, out var profileId)
            || profileId <= 0)
        {
            return new BadRequestObjectResult("profileId is not valid long number");
        }

        var profile = await _zwiftHttpClient.GetProfileDetailed(profileId, cancellationToken);
        if (profile is null)
        {
            return new NotFoundObjectResult($"Profile with id {profileId} not found");
        }

        var activities = await _zwiftHttpClient.ListActivities(profileId, cancellationToken: cancellationToken);

        return new OkObjectResult(new
        {
            ZwiftRacingUrl = $"https://zwiftracing.app/riders/{profileId}",
            ZwiftPowerUrl = $"https://zwiftpower.com/profile.php?z={profileId}",
            Profile = profile,
            LastActivities = activities
        });
    }
}
