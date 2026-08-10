using System.Diagnostics.CodeAnalysis;
using FitSyncHub.Common.Helpers;
using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Abstractions;
using FitSyncHub.Zwift.Models.FRR;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftFRRTourVELORatingHttpTriggerFunction
{
    private readonly IFlammeRougeRacingHttpClient _flammeRougeRacingHttpClient;
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ZwiftRacingHttpClient _zwiftRacingHttpClient;
    private readonly ILogger<ZwiftFRRTourVELORatingHttpTriggerFunction> _logger;

    public ZwiftFRRTourVELORatingHttpTriggerFunction(
        IFlammeRougeRacingHttpClient flammeRougeRacingHttpClient,
        ZwiftHttpClient zwiftHttpClient,
        ZwiftRacingHttpClient zwiftRacingHttpClient,
        ILogger<ZwiftFRRTourVELORatingHttpTriggerFunction> logger)
    {
        _flammeRougeRacingHttpClient = flammeRougeRacingHttpClient;
        _zwiftHttpClient = zwiftHttpClient;
        _zwiftRacingHttpClient = zwiftRacingHttpClient;
        _logger = logger;
    }

#if DEBUG
    [Function(nameof(ZwiftFRRTourVELORatingHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-frr-tour-vELO-rating")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var category = req.Query["category"];
        var eventUrlQueryParam = req.Query["eventUrl"];

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

        if (ShouldFilterRiders(eventUrlQueryParam, out var eventUrl))
        {
            var @event = await _zwiftHttpClient.GetEvent(eventUrl.ToString(), cancellationToken);

            HashSet<long> entrantIds = [];
            foreach (var subgroup in @event.EventSubgroups)
            {
                var entrants = await _zwiftHttpClient.GetEventSubgroupEntrants(subgroup.Id, cancellationToken: cancellationToken);
                entrantIds.UnionWith(entrants.Select(x => x.Id));
            }

            riders = [.. riders.Where(entrantIds.Contains)];
        }

        var result = await GetRidersVELO(riders, cancellationToken);

        result = [.. result
            .OrderByDescending(x => x.MaxVELO)
            .ThenByDescending(x => x.FtpPerKg)
        ];

        return new OkObjectResult(result);
    }

    private bool ShouldFilterRiders(StringValues eventUrl, [NotNullWhen(true)] out Uri? parsedUrl)
    {
        if (string.IsNullOrWhiteSpace(eventUrl))
        {
            parsedUrl = default;
            return false;
        }

        // should be valid if specified
        if (!Uri.TryCreate(eventUrl, UriKind.Absolute, out parsedUrl))
        {
            _logger.LogError("Wrong '{EventUrl}' url", StringHelper.Sanitize(eventUrl));
            return false;
        }

        return true;
    }

    private static IEnumerable<FlammeRougeRacingCategory> ParseCategories(StringValues category)
    {
        foreach (var categoryQueryParam in category)
        {
            if (!Enum.TryParse<FlammeRougeRacingCategory>(categoryQueryParam, ignoreCase: true, out var parsedFRRCategory))
            {
                throw new ArgumentOutOfRangeException(nameof(category), "Cannot parse FRR category {Category}", categoryQueryParam);
            }

            yield return parsedFRRCategory;
        }
    }

    private async Task<List<ZwiftEventVELORatingResponseItem>> GetRidersVELO(
        IReadOnlyCollection<long> riderIds,
        CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;

        List<ZwiftEventVELORatingResponseItem> items = [];

        foreach (var riderIdsChunk in riderIds.Chunk(5))
        {
            List<Task<ZwiftEventVELORatingResponseItem>> tasks = [];
            foreach (var riderId in riderIdsChunk)
            {
                tasks.Add(GetRiderVELO(riderId, year, cancellationToken));
            }

            await foreach (var item in Task.WhenEach(tasks))
            {
                items.Add(await item);
            }
        }

        return items;
    }

    private async Task<ZwiftEventVELORatingResponseItem> GetRiderVELO(
        long riderId,
        int year,
        CancellationToken cancellationToken)
    {
        var getHistoryTask = _zwiftRacingHttpClient.GetRiderHistory(riderId, year: year, cancellationToken);
        var getProfileTask = _zwiftHttpClient.GetProfile(riderId, cancellationToken);

        await Task.WhenAll(getHistoryTask, getProfileTask);

        var history = await getHistoryTask;
        var profile = await getProfileTask;

        var rider = new ZwiftEntrantResponseModel
        {
            Id = profile.Id,
            PublicId = profile.PublicId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Age = profile.Age,
            WeightInGrams = profile.Weight,
            HeightInMillimeters = profile.Height,
            Ftp = profile.Ftp
        };

        return ZwiftEventVELORatingResponseItem.Initialize(rider, history);
    }
}
