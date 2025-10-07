using System.Text;
using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ZwiftEventVELORatingHttpTriggerFunction
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
        string? zwiftEventUrl = req.Query["eventUrl"];
        string? subcategory = req.Query["subcategory"];

        if (string.IsNullOrWhiteSpace(cookie)
            || string.IsNullOrWhiteSpace(zwiftEventUrl)
            || string.IsNullOrWhiteSpace(subcategory))
        {
            return new BadRequestObjectResult("wrong request");
        }

        if (!Uri.TryCreate(zwiftEventUrl, UriKind.Absolute, out _))
        {
            return new BadRequestObjectResult("wrong url");
        }

        var entrants = await _zwiftEventsService.GetEntrants(zwiftEventUrl, subcategory, cancellationToken);

        var year = DateTime.UtcNow.Year;
        List<ZwiftEventVELORatingResponseItem> items = [];

        foreach (var rider in entrants)
        {
            var history = await _zwiftRacingHttpClient.GetRiderHistory(rider.Id, year: year, cancellationToken: cancellationToken);

            var maxVelo = history.History.Max(x => x.Rating);
            var minVelo = history.History.Min(x => x.Rating);
            var velo = history.History
                    .OrderByDescending(x => x.UpdatedAt)
                    .FirstOrDefault()?.Rating;

            var ftpPerKg = rider.Ftp / (rider.Weight / 1000.0);

            items.Add(new ZwiftEventVELORatingResponseItem
            {
                Id = rider.Id,
                FirstName = rider.FirstName,
                LastName = rider.LastName,
                Age = rider.Age,
                Weight = rider.Weight / 1000.0,
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

        string? format = req.Query["format"];
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csvLines = new List<string>
            {
                "Id,FirstName,LastName,Age,Weight,FtpPerKg,Best5Sec,Best15Sec,Best30Sec,Best1Min,Best2Min,Best5Min,Best20Min,MaxVELO,MinVELO,VELO\r\n"
            };
            csvLines.AddRange(items.Select(item =>
            {
                var sb = new StringBuilder();
                sb.Append($"{item.Id},");
                sb.Append($"{item.FirstName},");
                sb.Append($"{item.LastName},");
                sb.Append($"{item.Age},");
                sb.Append($"{item.Weight:F2},");
                sb.Append($"{item.FtpPerKg:F2},");
                sb.Append($"{item.Best5Sec:F2},");
                sb.Append($"{item.Best15Sec:F2},");
                sb.Append($"{item.Best30Sec:F2},");
                sb.Append($"{item.Best1Min:F2},");
                sb.Append($"{item.Best2Min:F2},");
                sb.Append($"{item.Best5Min:F2},");
                sb.Append($"{item.Best20Min:F2},");
                sb.Append($"{item.MaxVELO:F2},");
                sb.Append($"{item.MinVELO:F2},");
                sb.Append($"{item.VELO:F2}");
                return sb.ToString();
            }));
            var csvContent = string.Join(Environment.NewLine, csvLines);

            return new OkObjectResult(csvContent);
        }

        if (string.IsNullOrWhiteSpace(format) || string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
        {
            var jsonResult = new ZwiftEventVELORatingResponse
            {
                Year = DateTime.UtcNow.Year,
                Items = [.. items.OrderByDescending(x => x.MaxVELO)],
            };

            return new OkObjectResult(jsonResult);
        }

        return new BadRequestObjectResult("wrong format");
    }

    private static double? GetWkgValue(ZwiftRacingRiderResponse history, Func<ZwiftRacingHistoryEntry, double?> wkgSelector)
    {
        return history.History
            .Select(x => wkgSelector(x))
            .Where(x => x.HasValue)
            .OrderByDescending(x => x)
            .FirstOrDefault();
    }
}

public record ZwiftEventVELORatingResponse
{
    public required int Year { get; init; }
    public required ZwiftEventVELORatingResponseItem[] Items { get; init; }
    public int Count => Items.Length;

}

public record ZwiftEventVELORatingResponseItem
{
    public required int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required int Age { get; init; }
    public required double Weight { get; init; }
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
