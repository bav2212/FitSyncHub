using FitSyncHub.Common.Extensions;
using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions.Zwift;

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
        string? eventUrl = req.Query["eventUrl"];
        string? subcategory = req.Query["subcategory"];

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

        foreach (var entrantsChunk in entrants.Chunk(5))
        {
            List<Task<ZwiftEventVELORatingResponseItem>> tasks = [];
            foreach (var entrant in entrantsChunk)
            {
                tasks.Add(GetEntrantVELO(entrant, year, cancellationToken));
            }

            await foreach (var item in Task.WhenEach(tasks))
            {
                items.Add(await item);
            }
        }

        return items;
    }

    private async Task<ZwiftEventVELORatingResponseItem> GetEntrantVELO(
        ZwiftEntrantResponseModel rider,
        int year,
        CancellationToken cancellationToken)
    {
        var history = await _zwiftRacingHttpClient
                .GetRiderHistory(rider.Id, year: year, cancellationToken);

        return ZwiftEventVELORatingResponseItem.Initialize(rider, history);
    }
}

public sealed record ZwiftEventVELORatingResponseItem
{
    public static ZwiftEventVELORatingResponseItem Initialize(
        ZwiftEntrantResponseModel rider,
        ZwiftRacingRiderResponse? history)
    {
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
            PublicId = rider.PublicId,
            FirstName = rider.FirstName.Trim(),
            LastName = rider.LastName.Trim(),
            Age = rider.Age,
            Weight = weigth,
            Height = height,
            FtpPerKg = Math.Round(ftpPerKg, 2),
            Best5Sec = GetRoundedWkgValue(history, x => x.Wkg5),
            Best15Sec = GetRoundedWkgValue(history, x => x.Wkg15),
            Best30Sec = GetRoundedWkgValue(history, x => x.Wkg30),
            Best1Min = GetRoundedWkgValue(history, x => x.Wkg60),
            Best2Min = GetRoundedWkgValue(history, x => x.Wkg120),
            Best5Min = GetRoundedWkgValue(history, x => x.Wkg300),
            Best20Min = GetRoundedWkgValue(history, x => x.Wkg1200),
            MaxVELO = RoundVELO(maxVelo),
            MinVELO = RoundVELO(minVelo),
            VELO = RoundVELO(velo),
        };

        static double? GetRoundedWkgValue(
            ZwiftRacingRiderResponse? history,
            Func<ZwiftRacingHistoryEntry, double?> wkgSelector)
        {
            if (history == null)
            {
                return default;
            }

            foreach (var item in history.History
                .Select(wkgSelector)
                .WhereNotNull()
                .OrderByDescending(x => x))
            {
                // first item only, that's expected
                return Math.Round(item, 2);
            }

            // null if no items
            return default;
        }

        static double? RoundVELO(double? input) => input.HasValue ? Math.Round(input.Value, 0) : null;
    }

    public required long Id { get; init; }
    public required string PublicId { get; init; }
    public string ZwiftRacingUrl => $"https://zwiftracing.app/riders/{Id}";
    public string ZwiftPowerUrl => $"https://zwiftpower.com/profile.php?z={Id}";
    public string ZwiftUrl => $"https://zwift.com/athlete/{PublicId}";
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required int Age { get; init; }
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
