using FitSyncHub.Common.Extensions;
using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public sealed class ZwiftEventVELORatingHttpTriggerFunction
{
    private readonly ZwiftEventsService _zwiftEventsService;
    private readonly ZwiftRacingHttpClient _zwiftRacingHttpClient;

    public ZwiftEventVELORatingHttpTriggerFunction(
        ZwiftEventsService zwiftEventsService,
        ZwiftRacingHttpClient zwiftRacingHttpClient)
    {
        _zwiftEventsService = zwiftEventsService;
        _zwiftRacingHttpClient = zwiftRacingHttpClient;
    }

#if DEBUG
    [Function(nameof(ZwiftEventVELORatingHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-event-vELO-rating")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? cookie = req.Query["cookie"];
        string? eventUrl = req.Query["eventUrl"];
        string? subcategory = req.Query["subcategory"];

        if (string.IsNullOrWhiteSpace(cookie)
            || string.IsNullOrWhiteSpace(eventUrl))
        {
            return new BadRequestObjectResult($"Specify params: {nameof(cookie)}, {nameof(eventUrl)}");
        }

        if (!Uri.TryCreate(eventUrl, UriKind.Absolute, out _))
        {
            return new BadRequestObjectResult($"Wrong '{nameof(eventUrl)}' url");
        }

        var entrants = await _zwiftEventsService
            .GetEntrants(eventUrl, subcategory, includeMyself: true, cancellationToken);

        var result = await GetEntrantsVELO(entrants, cancellationToken);
        result = [.. result.OrderByDescending(x => x.MaxVELO)];

        return new OkObjectResult(result);
    }

    private async Task<List<ZwiftEventVELORatingResponseItem>> GetEntrantsVELO(
        IReadOnlyCollection<ZwiftEntrantResponseModel> entrants,
        CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;

        List<ZwiftEventVELORatingResponseItem> items = [];
        foreach (var rider in entrants)
        {
            var history = await _zwiftRacingHttpClient
                .GetRiderHistory(rider.Id, year: year, cancellationToken);

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

public sealed record ZwiftEventVELORatingResponseItem
{
    public required long Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required uint Age { get; init; }
    public required double Weight { get; init; }
    public required double Height { get; init; }
    public required double? MaxVELO { get; init; }
    public required double? MinVELO { get; init; }
    public required double? VELO { get; init; }
    public required double FtpPerKg { get; init; }
    public required double? Best5Sec { get; init; }
    public required double? Best15Sec { get; init; }
    public required double? Best30Sec { get; init; }
    public required double? Best1Min { get; init; }
    public required double? Best2Min { get; init; }
    public required double? Best5Min { get; init; }
    public required double? Best20Min { get; init; }
}
