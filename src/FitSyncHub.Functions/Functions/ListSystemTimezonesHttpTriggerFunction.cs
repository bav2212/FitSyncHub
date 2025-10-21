using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class ListSystemTimezonesHttpTriggerFunction
{
    private readonly ILogger<ListSystemTimezonesHttpTriggerFunction> _logger;

    public ListSystemTimezonesHttpTriggerFunction(ILogger<ListSystemTimezonesHttpTriggerFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ListSystemTimezonesHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "list-time-zones")] HttpRequest req)
    {
        _ = req;

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

        _logger.LogInformation("TimeZoneInfo details: Id={Id}, DisplayName={Display}, BaseOffset={Offset}, SupportsDST={DST}",
            timezone.Id, timezone.DisplayName, timezone.BaseUtcOffset, timezone.SupportsDaylightSavingTime);

        var nowUtc = System.DateTime.UtcNow;
        _logger.LogInformation("UTC now: {NowUtc}", nowUtc);
        _logger.LogInformation("Converted: {LocalNow}", TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));
        _logger.LogInformation("Offset via GetUtcOffset: {Offset}", timezone.GetUtcOffset(nowUtc));

        var eventStartTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone);
        var eventStartOffset = new DateTimeOffset(eventStartTime, timezone.GetUtcOffset(nowUtc));

        _logger.LogInformation("Converted with custom string format: {Format}", eventStartOffset.ToString("yyyy-MM-dd HH:mm:ss zzz"));


        return new OkObjectResult(TimeZoneInfo.GetSystemTimeZones());
    }
}
