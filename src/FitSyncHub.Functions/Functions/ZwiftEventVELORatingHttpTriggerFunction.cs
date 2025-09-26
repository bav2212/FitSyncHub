using FitSyncHub.Zwift.HttpClients;
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
                MaxVELO = maxVelo,
                MinVELO = minVelo,
                VELO = velo,
            });
        }

        var result = new ZwiftEventVELORatingResponse
        {
            Year = DateTime.UtcNow.Year,
            Items = [.. items.OrderByDescending(x => x.MaxVELO)],
        };

        return new OkObjectResult(result);
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
}
