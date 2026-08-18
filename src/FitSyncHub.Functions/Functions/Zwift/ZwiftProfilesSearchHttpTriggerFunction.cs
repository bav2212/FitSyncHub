using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Profiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftProfilesSearchHttpTriggerFunction
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ILogger<ZwiftProfilesSearchHttpTriggerFunction> _logger;

    public ZwiftProfilesSearchHttpTriggerFunction(
        ZwiftHttpClient zwiftHttpClient,
        ILogger<ZwiftProfilesSearchHttpTriggerFunction> logger)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _logger = logger;
    }

    [Function(nameof(ZwiftProfilesSearchHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-profiles-search")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var searchTextQueryParam = req.Query["searchText"];
        if (string.IsNullOrWhiteSpace(searchTextQueryParam))
        {
            return new BadRequestObjectResult("Specify params: searchText");
        }

        var request = new ZwiftSearchProfileRequest
        {
            SearchText = searchTextQueryParam!
        };

        var response = await _zwiftHttpClient.SearchProfiles(request, cancellationToken);
        if (response is null)
        {
            return new NotFoundObjectResult($"Profiles with search text {searchTextQueryParam} not found");
        }

        return new OkObjectResult(
            new
            {
                Profiles = response.Profiles
                // real account will be on the top
                .OrderByDescending(x => x.AchievementLevel)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.Age,
                    AchievementLevel = x.AchievementLevel / 100.0,
                    Weight = x.Weight / 1000.0,
                    Height = x.Height / 1000.0,
                })
            }
        );
    }
}
