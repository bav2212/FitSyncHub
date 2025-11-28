using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public sealed class ZwiftEventRidersCompetitionMetricsHttpTriggerFunction
{
    private readonly ZwiftEventsService _zwiftEventsService;

    public ZwiftEventRidersCompetitionMetricsHttpTriggerFunction(
        ZwiftEventsService zwiftEventsService)
    {
        _zwiftEventsService = zwiftEventsService;
    }

    [Function(nameof(ZwiftEventRidersCompetitionMetricsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-event-competition-metrics")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? zwiftEventUrl = req.Query["eventUrl"];
        string? subcategory = req.Query["subcategory"];

        if (string.IsNullOrWhiteSpace(zwiftEventUrl)
            || string.IsNullOrWhiteSpace(subcategory))
        {
            return new BadRequestObjectResult("wrong request");
        }

        if (!Uri.TryCreate(zwiftEventUrl, UriKind.Absolute, out _))
        {
            return new BadRequestObjectResult("wrong url");
        }

        var entrants = await _zwiftEventsService
            .GetEntrants(zwiftEventUrl, subcategory, includeMyself: true, cancellationToken: cancellationToken);

        var result = await GetRidersCompetitionMetrics(entrants, cancellationToken);
        result = [.. result.OrderByDescending(x => x.RacingScore)];

        return new OkObjectResult(result);
    }

    private async Task<List<ZwiftEventRidersCompetitionMetricsResponseItem>> GetRidersCompetitionMetrics(
        IReadOnlyCollection<ZwiftEntrantResponseModel> entrants,
        CancellationToken cancellationToken)
    {
        List<ZwiftEventRidersCompetitionMetricsResponseItem> items = [];
        foreach (var rider in entrants)
        {
            var competitionMetrics = await _zwiftEventsService
                .GetCompetitionMetrics(rider.Id, cancellationToken: cancellationToken);

            var weigth = rider.WeightInGrams / 1000.0;
            var height = rider.HeightInMillimeters / 1000.0;
            var ftpPerKg = rider.Ftp / weigth;

            items.Add(new ZwiftEventRidersCompetitionMetricsResponseItem
            {
                Id = rider.Id,
                FirstName = rider.FirstName,
                LastName = rider.LastName,
                Gender = competitionMetrics.Male ? "male" : "female",
                Age = rider.Age,
                Weight = weigth,
                Height = height,
                FtpPerKg = ftpPerKg,
                Category = competitionMetrics.Category,
                RacingScore = competitionMetrics.RacingScore,
            });
        }

        return items;
    }
}

public sealed record ZwiftEventRidersCompetitionMetricsResponseItem
{
    public required long Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required uint Age { get; init; }
    public required double Weight { get; init; }
    public required double Height { get; init; }
    public required double FtpPerKg { get; init; }
    public required string? Category { get; init; }
    public required double? RacingScore { get; init; }
    public required string Gender { get; init; }
}
