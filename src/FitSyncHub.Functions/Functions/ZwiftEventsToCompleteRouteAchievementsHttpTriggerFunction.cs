using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction
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

        DateTimeOffset from;
        DateTimeOffset to;

        string? fromQueryParameter = req.Query["from"];
        from = DateOnly.TryParse(fromQueryParameter, out var fromDateOnly)
            ? new DateTimeOffset(new DateTime(fromDateOnly, TimeOnly.MinValue))
            : DateTimeOffset.UtcNow;

        string? toQueryParameter = req.Query["to"];
        to = DateOnly.TryParse(toQueryParameter, out var toDateOnly)
            ? new DateTimeOffset(new DateTime(toDateOnly, TimeOnly.MaxValue))
            : from.AddDays(7);

        string? forceQueryParameter = req.Query["force"];
        var force = bool.TryParse(forceQueryParameter, out var parsedForce) && parsedForce;

        if (to - from > TimeSpan.FromDays(7) && !force)
        {
            return new BadRequestObjectResult("Date range should not exceed 7 days, set 'force'=true in query to proceed");
        }

        var cyclingRouteToEventMapping = await _zwiftGameInfoService.GetUncompletedRouteToEventsMappingAchievements(
            from, to, cancellationToken);
        var sb = new StringBuilder();

        foreach (var (routeName, events) in cyclingRouteToEventMapping)
        {
            sb.AppendLine(routeName);

            foreach (var @event in events)
            {
                var eventStartTime =
                    TimeZoneInfo.ConvertTimeFromUtc(@event.EventStart, timezone);

                sb.AppendLine($" - {eventStartTime.ToString("yyyy-MM-dd HH:mm:ss zzz")}: '{@event.Name.Trim()}', Id: {@event.Id}");
            }

            sb.AppendLine();
        }

        return new OkObjectResult(sb.ToString());
    }
}
