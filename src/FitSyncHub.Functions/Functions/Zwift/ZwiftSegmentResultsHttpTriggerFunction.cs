using FitSyncHub.Zwift.HttpClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftSegmentResultsHttpTriggerFunction
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ILogger<ZwiftSegmentResultsHttpTriggerFunction> _logger;

    public ZwiftSegmentResultsHttpTriggerFunction(
        ZwiftHttpClient zwiftHttpClient,
        ILogger<ZwiftSegmentResultsHttpTriggerFunction> logger)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _logger = logger;
    }

#if DEBUG
    [Function(nameof(ZwiftSegmentResultsHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-segment-results")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var segmentIdQueryParam = req.Query["segmentId"];
        if (string.IsNullOrWhiteSpace(segmentIdQueryParam))
        {
            return new BadRequestObjectResult("Specify params: segmentId");
        }

        if (!long.TryParse(segmentIdQueryParam, out var segmentId)
            || segmentId <= 0)
        {
            return new BadRequestObjectResult("segmentId is not valid long number");
        }

        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);
        var segmentIds = gameInfo.Segments.Select(x => x.Id).ToHashSet();
        if (!segmentIds.Contains(segmentId))
        {
            return new BadRequestObjectResult("segment with specified id doesn't exists");
        }

        long profileId;

        var profileIdQueryParam = req.Query["profileId"];
        if (string.IsNullOrWhiteSpace(profileIdQueryParam))
        {
            var profileMe = await _zwiftHttpClient.GetProfileMe(cancellationToken);
            profileId = profileMe.Id;
        }
        else if (!long.TryParse(profileIdQueryParam, out profileId) && profileId <= 0)
        {
            return new BadRequestObjectResult("profileId is not valid long number");
        }

        var profile = await _zwiftHttpClient.GetProfileDetailed(profileId, cancellationToken);
        if (profile is null)
        {
            return new NotFoundObjectResult($"Profile with id {profileId} not found");
        }

        var to = DateTime.UtcNow;
        var from = profile.CreatedOn!.Value;

        var segmentResults = await _zwiftHttpClient
            .GetSegmentResults(profileId, segmentId, from, to, cancellationToken);

        var results = segmentResults
            .Select(x => new
            {
                x.FirstName,
                x.LastName,
                x.PowerType,
                Weight = x.Weight / 1000.0,
                x.AvgPower,
                x.Male,
                Elapsed = TimeSpan.FromMilliseconds(x.Elapsed),
                // took 1414016074400 from sauce code
                Date = DateTimeOffset.FromUnixTimeMilliseconds(1414016074400) + TimeSpan.FromMilliseconds(x.WorldTime)
            })
            .OrderBy(x => x.Elapsed)
            .ToList();

        return new OkObjectResult(results);
    }
}
