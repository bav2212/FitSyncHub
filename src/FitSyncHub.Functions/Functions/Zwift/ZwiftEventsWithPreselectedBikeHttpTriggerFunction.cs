using System.Text.Json.Serialization;
using FitSyncHub.Common.Helpers;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftEventsWithPreselectedBikeHttpTriggerFunction
{
    private readonly ZwiftGameInfoService _zwiftGameInfoService;
    private readonly ILogger<ZwiftEventsWithPreselectedBikeHttpTriggerFunction> _logger;

    public ZwiftEventsWithPreselectedBikeHttpTriggerFunction(
        ZwiftGameInfoService zwiftGameInfoService,
        ILogger<ZwiftEventsWithPreselectedBikeHttpTriggerFunction> logger)
    {
        _zwiftGameInfoService = zwiftGameInfoService;
        _logger = logger;
    }

    [Function(nameof(ZwiftEventsWithPreselectedBikeHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-events-with-preselected-bike")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? timezoneQueryParameter = req.Query["timezone"];

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Timezone request query parameter: {QueryParam}", StringHelper.Sanitize(timezoneQueryParameter));
        }

        TimeZoneInfo timezone;
        if (timezoneQueryParameter is { } && TimeZoneInfo.TryFindSystemTimeZoneById(timezoneQueryParameter, out var parsedTimeZone))
        {
#pragma warning disable CA1873 // Avoid potentially expensive logging
            _logger.LogInformation("Using timezone: {TimeZone}", parsedTimeZone);
#pragma warning restore CA1873 // Avoid potentially expensive logging
            timezone = parsedTimeZone;
        }
        else
        {
            _logger.LogWarning("Invalid or missing timezone parameter, defaulting to UTC");
            timezone = TimeZoneInfo.Utc;
        }

        string? fromQueryParameter = req.Query["from"];
        var from = DateOnly.TryParse(fromQueryParameter, out var fromDateOnly)
            ? new DateTimeOffset(new DateTime(fromDateOnly, TimeOnly.MinValue))
            : DateTimeOffset.UtcNow;

        string? toQueryParameter = req.Query["to"];
        var to = DateOnly.TryParse(toQueryParameter, out var toDateOnly)
            ? new DateTimeOffset(new DateTime(toDateOnly, TimeOnly.MaxValue))
            : from.AddDays(7);

        string? forceQueryParameter = req.Query["force"];
        var force = bool.TryParse(forceQueryParameter, out var parsedForce) && parsedForce;

        if (to - from > TimeSpan.FromDays(7) && !force)
        {
            return new BadRequestObjectResult("Date range should not exceed 7 days, set 'force'=true in query to proceed");
        }

        var zwiftEventsWithPreselectedBike = await _zwiftGameInfoService
            .GetZwiftEventsWithPreselectedBike(from, to, cancellationToken);

        var result = Convert(zwiftEventsWithPreselectedBike, timezone)
            .OrderBy(x => x.BikeFrame)
            .ToList();

        return new OkObjectResult(result);
    }

    private static IEnumerable<ZwiftEventsWithPreselectedBikeResponse> Convert(
        List<ZwiftEventWithPreselectedBikeTuple> zwiftEventsWithPreselectedBike,
        TimeZoneInfo timezone)
    {
        var groupedByBikeFrame = zwiftEventsWithPreselectedBike
            .Where(x => x.BikeFrame is not null)
            .GroupBy(e => e.BikeFrame!)
            .ToList();

        foreach (var item in groupedByBikeFrame)
        {
            var bikeFrame = item.Key;

            yield return new ZwiftEventsWithPreselectedBikeResponse
            {
                BikeFrame = bikeFrame.Name,
                Events = [.. item.Select(x =>
                {
                    var e = x.Event;
                    var route = x.Route;

                    var eventStartTime = TimeZoneInfo.ConvertTimeFromUtc(e.EventStart, timezone);
                    var eventStartOffset = new DateTimeOffset(eventStartTime, timezone.GetUtcOffset(e.EventStart));

                    double distanceImMeters;
                    double? elevation;
                    // can be null or 0
                    if (e.Laps is { } eventLaps && eventLaps > 0)
                    {
                        distanceImMeters = route.LeadinDistanceInMeters + (route.DistanceInMeters * eventLaps);
                        elevation = route.LeadinAscentInMeters + (route.AscentInMeters * eventLaps);
                    }
                    // can be null or 0
                    else if (e.DistanceInMeters is { } eventDistanceInMeters && eventDistanceInMeters > 0)
                    {
                        distanceImMeters = eventDistanceInMeters;
                        elevation = default;
                    }
                    else
                    {
                        distanceImMeters = route.TotalDistanceInMeters;
                        elevation = route.TotalAscentInMeters;
                    }

                    var item = new ZwiftEventsWithPreselectedBikeEventItem
                    {
                        EventStart = eventStartOffset,
                        EventName = e.Name,
                        Id = e.Id,
                        RouteName = route.Name,
                        Distance = Math.Round(distanceImMeters / 1000.0, 1),
                        Elevation = elevation is null ? null : Math.Round(elevation.Value),
                    };

                    if (e.DurationInSeconds > 0)
                    {
                        item = item with { DurationInSeconds = e.DurationInSeconds };
                    }

                    return item;
                })]
            };
        }
    }
}

public sealed record ZwiftEventsWithPreselectedBikeResponse
{
    public required string BikeFrame { get; init; }
    public required List<ZwiftEventsWithPreselectedBikeEventItem> Events { get; init; }
}

public sealed record ZwiftEventsWithPreselectedBikeEventItem
{
    public required DateTimeOffset EventStart { get; init; }
    public required string EventName { get; init; }
    public required long Id { get; init; }
    public required string RouteName { get; init; }
    public required double Distance { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required double? Elevation { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double? DurationInSeconds { get; init; }
    public string ZwiftUrl => $"https://www.zwift.com/events/view/{Id}";
}
