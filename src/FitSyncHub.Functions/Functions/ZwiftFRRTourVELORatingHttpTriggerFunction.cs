using System.Diagnostics.CodeAnalysis;
using FitSyncHub.Common.Extensions;
using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Abstractions;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using FitSyncHub.Zwift.Models.FRR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Functions.Functions;

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
            _logger.LogError("Wrong '{EventUrl}' url", eventUrl!);
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
        var historyTask = _zwiftRacingHttpClient.GetRiderHistory(riderId, year: year, cancellationToken);
        var riderTask = _zwiftHttpClient.GetProfile(riderId, cancellationToken);

        await Task.WhenAll(historyTask, riderTask);

        var history = await historyTask;
        var rider = await riderTask;

        var maxVelo = history?.History.Max(x => x.Rating);
        var minVelo = history?.History.Min(x => x.Rating);
        var velo = history?.History
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefault()?.Rating;

        var weigth = rider.WeightInGrams / 1000.0;
        var height = rider.HeightInMillimeters / 1000.0;

        var ftpPerKg = rider.Ftp / weigth;

        return new ZwiftEventVELORatingResponseItem
        {
            Id = rider.Id,
            FirstName = rider.FirstName,
            LastName = rider.LastName,
            Age = rider.Age,
            Weight = weigth,
            Height = height,
            FtpPerKg = ftpPerKg,
            Best5Sec = GetWkgValue(history, x => x.Wkg5),
            Best15Sec = GetWkgValue(history, x => x.Wkg15),
            Best30Sec = GetWkgValue(history, x => x.Wkg30),
            Best1Min = GetWkgValue(history, x => x.Wkg60),
            Best2Min = GetWkgValue(history, x => x.Wkg120),
            Best5Min = GetWkgValue(history, x => x.Wkg300),
            Best20Min = GetWkgValue(history, x => x.Wkg1200),
            MaxVELO = maxVelo,
            MinVELO = minVelo,
            VELO = velo,
        };
    }

    private static double? GetWkgValue(
        ZwiftRacingRiderResponse? history,
        Func<ZwiftRacingHistoryEntry, double?> wkgSelector)
    {
        if (history == null)
        {
            return default;
        }

        return history.History
            .Select(wkgSelector)
            .WhereNotNull()
            .OrderByDescending(x => x)
            .FirstOrNull();
    }
}
