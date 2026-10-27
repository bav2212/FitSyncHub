using System.Text.Json.Serialization;
using FitSyncHub.Common.Helpers;
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
        DateTimeOffset from;
        if (string.IsNullOrWhiteSpace(fromQueryParameter))
        {
            from = DateTimeOffset.UtcNow;
        }
        else if (DateOnly.TryParseExact(fromQueryParameter, "yyyy-MM-dd", out var fromDateOnly))
        {
            from = new DateTimeOffset(new DateTime(fromDateOnly, TimeOnly.MinValue));
        }
        else
        {
            return new BadRequestObjectResult("Can not parse 'from' date");
        }

        string? toQueryParameter = req.Query["to"];
        DateTimeOffset to;
        if (string.IsNullOrWhiteSpace(toQueryParameter))
        {
            to = from.AddDays(7);
        }
        else if (DateOnly.TryParseExact(toQueryParameter, "yyyy-MM-dd", out var toDateOnly))
        {
            to = new DateTimeOffset(new DateTime(toDateOnly, TimeOnly.MaxValue));
        }
        else
        {
            return new BadRequestObjectResult("Can not parse 'to' date");
        }

        if (to < from)
        {
            return new BadRequestObjectResult("'to' should be bigger then 'from'");
        }

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
                WorldName = route.WorldName,
                RouteName = route.Name,
                Events = [.. events
                    .SelectMany(@event => @event.EventSubgroups
                        .Where(x => x.RouteId == route.Id)
                        .Select(eventSubgroup =>
                        {
                            var eventStartTime = TimeZoneInfo.ConvertTimeFromUtc(eventSubgroup.EventSubgroupStart, timezone);
                            var eventStartOffset = new DateTimeOffset(eventStartTime, timezone.GetUtcOffset(eventSubgroup.EventSubgroupStart));

                            double distanceImMeters;
                            double? elevation;
                            // can be null or 0
                            if (eventSubgroup.Laps is { } eventLaps && eventLaps > 0)
                            {
                                distanceImMeters = route.LeadinDistanceInMeters + (route.DistanceInMeters * eventLaps);
                                elevation = route.LeadinAscentInMeters + (route.AscentInMeters * eventLaps);
                            }
                            // can be null or 0
                            else if (eventSubgroup.DistanceInMeters is { } eventDistanceInMeters && eventDistanceInMeters > 0)
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
                                EventName = eventSubgroup.Name,
                                Id = @event.Id,
                                Distance = Math.Round(distanceImMeters / 1000.0, 1),
                                Elevation = elevation is null ? null : Math.Round(elevation.Value),
                            };

                            if (eventSubgroup.DurationInSeconds > 0)
                            {
                                item = item with { DurationInSeconds = eventSubgroup.DurationInSeconds };
                            }

                            return item;
                        })
                    )
                    .DistinctBy(x => x.Id)]
            };
        }
    }
}

public sealed record ZwiftEventsToCompleteRouteAchievementsResponse
{
    public required string WorldName { get; init; }
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
