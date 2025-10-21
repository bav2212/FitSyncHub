using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction
{
    private readonly ZwiftGameInfoService _zwiftGameInfoService;

    public ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction(
        ZwiftGameInfoService zwiftGameInfoService)
    {
        _zwiftGameInfoService = zwiftGameInfoService;
    }

    [Function(nameof(ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-events-to-complete-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? timezoneQueryParameter = req.Query["timezone"];
        // In the receiving application, convert UTC to a specific time zone

        var timezone = timezoneQueryParameter is { } && TimeZoneInfo.TryFindSystemTimeZoneById(timezoneQueryParameter, out var parsedTimeZone)
            ? parsedTimeZone
            : TimeZoneInfo.Utc;

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
