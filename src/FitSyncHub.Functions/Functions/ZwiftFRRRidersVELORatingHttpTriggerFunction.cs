using FitSyncHub.Common.Extensions;
using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public sealed class ZwiftFRRRidersVELORatingHttpTriggerFunction
{
    private readonly FlammeRougeRacingHttpClient _flammeRougeRacingHttpClient;
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ZwiftRacingHttpClient _zwiftRacingHttpClient;

    public ZwiftFRRRidersVELORatingHttpTriggerFunction(
        FlammeRougeRacingHttpClient flammeRougeRacingHttpClient,
        ZwiftHttpClient zwiftHttpClient,
        ZwiftRacingHttpClient zwiftRacingHttpClient)
    {
        _flammeRougeRacingHttpClient = flammeRougeRacingHttpClient;
        _zwiftHttpClient = zwiftHttpClient;
        _zwiftRacingHttpClient = zwiftRacingHttpClient;
    }

#if DEBUG
    [Function(nameof(ZwiftFRRRidersVELORatingHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-frr-tour-vELO-rating")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? cookie = req.Query["cookie"];
        string? category = req.Query["category"];

        if (string.IsNullOrWhiteSpace(cookie)
            || string.IsNullOrWhiteSpace(category))
        {
            return new BadRequestObjectResult($"Specify params: {nameof(cookie)}, {nameof(category)}");
        }

        if (!Enum.TryParse<FlammeRougeRacingCategory>(category, ignoreCase: true, out var parsedFRRCategory))
        {
            return new BadRequestObjectResult($"Cannot parse FRR category");
        }

        var riders = await _flammeRougeRacingHttpClient
            .GetTourRegisteredRiders(parsedFRRCategory, cancellationToken);

        var result = await GetRidersVELO(riders, cancellationToken);
        result = [.. result.OrderByDescending(x => x.MaxVELO)];

        return new OkObjectResult(result);
    }

    private async Task<List<ZwiftEventVELORatingResponseItem>> GetRidersVELO(
        IReadOnlyCollection<long> riderIds,
        CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;

        List<ZwiftEventVELORatingResponseItem> items = [];
        foreach (var riderId in riderIds)
        {
            var historyTask = _zwiftRacingHttpClient
                .GetRiderHistory(riderId, year: year, cancellationToken);
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

            items.Add(new ZwiftEventVELORatingResponseItem
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
            });
        }

        return items;
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
