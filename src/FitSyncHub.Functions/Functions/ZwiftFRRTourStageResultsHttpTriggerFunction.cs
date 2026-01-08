using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.Models.FRR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Functions.Functions;

public sealed class ZwiftFRRTourStageResultsHttpTriggerFunction
{
    private readonly FlammeRougeRacingHttpClient _flammeRougeRacingHttpClient;
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ILogger<ZwiftFRRTourStageResultsHttpTriggerFunction> _logger;

    public ZwiftFRRTourStageResultsHttpTriggerFunction(
        FlammeRougeRacingHttpClient flammeRougeRacingHttpClient,
        ZwiftHttpClient zwiftHttpClient,
        ILogger<ZwiftFRRTourStageResultsHttpTriggerFunction> logger)
    {
        _flammeRougeRacingHttpClient = flammeRougeRacingHttpClient;
        _zwiftHttpClient = zwiftHttpClient;
        _logger = logger;
    }

#if DEBUG
    [Function(nameof(ZwiftFRRTourStageResultsHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "zwift-frr-tour-stage-results")] HttpRequest req,
        [Microsoft.Azure.Functions.Worker.Http.FromBody] ZwiftFRRTourStageResultsRequest request,
        CancellationToken cancellationToken)
    {
        var category = req.Query["category"];
        if (string.IsNullOrWhiteSpace(category) || category.Count == 0)
        {
            return new BadRequestObjectResult($"Specify params: {nameof(category)}");
        }

        var tasksToGetRiders = ParseCategories(category)
            .ToHashSet()
            .Select(x => _flammeRougeRacingHttpClient.GetTourRegisteredRiders(x, cancellationToken))
            .ToList();

        List<long> riders = [];
        await foreach (var taskToGetRiders in Task.WhenEach(tasksToGetRiders))
        {
            var ridersPortion = await taskToGetRiders;
            riders.AddRange(ridersPortion);
        }

        var racesResults = await GetRacesResults([.. riders], request.Urls, cancellationToken);

        var isITT = racesResults.All(x => x.Key.Event.IsITT);
        var bestTimeOverAll = racesResults
            .SelectMany(x => x.Value)
            .Min(x => x.ActivityData.DurationInMilliseconds);

        var result = racesResults
            .Where(kv => kv.Value.Count > 0)
            .SelectMany(kv =>
            {
                var raceResults = kv.Value;

                var bestTime = isITT
                    ? bestTimeOverAll
                    : raceResults.Min(x => x.ActivityData.DurationInMilliseconds);

                var bestTimeSpan = TimeSpan.FromMilliseconds(bestTime);

                return raceResults.Select(x =>
                {
                    var duration = TimeSpan.FromMilliseconds(x.ActivityData.DurationInMilliseconds);

                    return new
                    {
                        RiderId = x.ProfileId,
                        FullName = $"{x.ProfileData.FirstName} {x.ProfileData.LastName}",
                        Duration = duration,
                        CritivalPower = x.CriticalP,
                        AvgPower = x.SensorData.AvgWatts,
                        AvgPowerPerKg = x.SensorData.AvgWatts / (x.ProfileData.WeightInGrams / 1000.0),
                        EGap = duration - bestTimeSpan,
                    };
                });
            })
            .OrderBy(x => x.EGap)
            .ToList();

        return new OkObjectResult(result);
    }

    private IEnumerable<FlammeRougeRacingCategory> ParseCategories(StringValues category)
    {
        foreach (var categoryQueryParam in category)
        {
            if (!Enum.TryParse<FlammeRougeRacingCategory>(categoryQueryParam, ignoreCase: true, out var parsedFRRCategory))
            {
                _logger.LogError("Cannot parse FRR category {Category}", categoryQueryParam);
                continue;
            }

            yield return parsedFRRCategory;
        }
    }

    private async Task<Dictionary<ZwiftEventEventSubgroupKey, List<ZwiftRaceResultEntryResponse>>> GetRacesResults(
        HashSet<long> riders,
        HashSet<string> urls,
        CancellationToken cancellationToken)
    {
        Dictionary<ZwiftEventEventSubgroupKey, List<ZwiftRaceResultEntryResponse>> result = [];

        foreach (var url in urls)
        {
            var @event = await _zwiftHttpClient.GetEvent(url, cancellationToken);
            if (@event == null)
            {
                continue;
            }

            foreach (var zwiftEventSubgroup in @event.EventSubgroups)
            {
                var subgroupResults = await _zwiftHttpClient
                    .GetEventSubgroupResults(zwiftEventSubgroup.Id, cancellationToken);

                // early exit if no results
                if (subgroupResults.Entries.Count == 0)
                {
                    return result;
                }

                var resultsPortion = subgroupResults.Entries
                    .Where(x => riders.Contains(x.ProfileId))
                    .ToList();

                if (resultsPortion.Count == 0)
                {
                    continue;
                }

                var key = new ZwiftEventEventSubgroupKey
                {
                    Event = @event,
                    Subgroup = zwiftEventSubgroup,
                    SubgroupResults = subgroupResults
                };

                result.Add(key, resultsPortion);
            }
        }

        return result;
    }

    private record ZwiftEventEventSubgroupKey
    {
        public required ZwiftEventResponse Event { get; init; }
        public required ZwiftEventSubgroupResponse Subgroup { get; init; }
        public required ZwiftRaceResultResponse SubgroupResults { get; init; }
    }
}

public sealed record ZwiftFRRTourStageResultsRequest
{
    public required HashSet<string> Urls { get; init; }
}
