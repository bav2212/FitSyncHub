using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction
{
    private readonly ZwiftGameInfoService _zwiftGameInfoService;
    private readonly ILogger<ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction> _logger;

    public ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction(
        ZwiftGameInfoService zwiftGameInfoService,
        ILogger<ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction> logger)
    {
        _zwiftGameInfoService = zwiftGameInfoService;
        _logger = logger;
    }

    [Function(nameof(ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-events-to-complete-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? timezoneQueryParameter = req.Query["timezone"];

        _logger.LogInformation("Timezone request query parameter: {QueryParam}", timezoneQueryParameter);

        TimeZoneInfo timezone;
        if (timezoneQueryParameter is { } && TimeZoneInfo.TryFindSystemTimeZoneById(timezoneQueryParameter, out var parsedTimeZone))
        {
            _logger.LogInformation("Using timezone: {TimeZone}", parsedTimeZone);
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

        var cyclingRouteToEventMapping = await _zwiftGameInfoService
            .GetUncompletedRouteToEventsMappingAchievements(from, to, cancellationToken);

        var result = Convert(cyclingRouteToEventMapping, timezone).ToList();

        return new OkObjectResult(result);
    }

    private static IEnumerable<ZwiftEventsToCompleteRouteAchievementsResponse> Convert(
        Dictionary<ZwiftRouteModel, List<ZwiftEventResponse>> cyclingRouteToEventMapping,
        TimeZoneInfo timezone)
    {
        foreach (var (route, events) in cyclingRouteToEventMapping)
        {
            // can ride non-public events anytime, do not need to list them
            if (!route.PublicEventsOnly)
            {
                continue;
            }

            yield return new ZwiftEventsToCompleteRouteAchievementsResponse
            {
                RouteName = route.Name,
                Events = events.ConvertAll(e =>
                {
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

                    var item = new ZwiftEventsToCompleteRouteAchievementsEventItem
                    {
                        EventStart = eventStartOffset,
                        EventName = e.Name,
                        Id = e.Id,
                        Distance = Math.Round(distanceImMeters / 1000.0, 1),
                        Elevation = elevation is null ? null : Math.Round(elevation.Value),
                    };

                    if (e.DurationInSeconds > 0)
                    {
                        item = item with { DurationInSeconds = e.DurationInSeconds };
                    }

                    return item;
                })
            };
        }
    }
}

public sealed record ZwiftEventsToCompleteRouteAchievementsResponse
{
    public required string RouteName { get; init; }
    public required List<ZwiftEventsToCompleteRouteAchievementsEventItem> Events { get; init; }
}

public sealed record ZwiftEventsToCompleteRouteAchievementsEventItem
{
    public required DateTimeOffset EventStart { get; init; }
    public required string EventName { get; init; }
    public required long Id { get; init; }
    public required double Distance { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required double? Elevation { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double? DurationInSeconds { get; init; }
    public string ZwiftUrl => $"https://www.zwift.com/events/view/{Id}";
}
