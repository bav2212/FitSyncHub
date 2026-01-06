using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
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

        var ridersTaskResult = await GetRacesResults(riders, request.Urls, cancellationToken);

        var result = ridersTaskResult
            .Select(x => new
            {
                RiderId = x.ProfileId,
                FullName = $"{x.ProfileData.FirstName} {x.ProfileData.LastName}",
                Duration = TimeSpan.FromMilliseconds(x.ActivityData.DurationInMilliseconds),
                CritivalPower = x.CriticalP,
                AvgPower = x.SensorData.AvgWatts,
                AvgPowerPerKg = x.SensorData.AvgWatts / (x.ProfileData.WeightInGrams / 1000.0),
            })
            .OrderBy(x => x.Duration)
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

    private async Task<List<ZwiftRaceResultEntryResponse>> GetRacesResults(
        List<long> riders,
        string[] urls,
        CancellationToken cancellationToken)
    {
        var ridersSet = riders.ToHashSet();
        List<ZwiftRaceResultEntryResponse> acc = [];

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
                    return acc;
                }

                var resultsPortion = subgroupResults.Entries
                    .Where(x => ridersSet.Contains(x.ProfileId))
                    .ToList();

                acc.AddRange(resultsPortion);
            }
        }

        return acc;
    }
}

public sealed record ZwiftFRRTourStageResultsRequest
{
    public required string[] Urls { get; init; }
}
