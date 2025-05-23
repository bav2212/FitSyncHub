using System.Text;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.IntervalsIcu;

public class WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction
{
    private readonly WhatsOnZwiftToIntervalsIcuService _zwiftToIntervalsIcuService;
    private readonly ILogger<WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction> _logger;

    public WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction(
        WhatsOnZwiftToIntervalsIcuService zwiftToIntervalsIcuService,
        ILogger<WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction> logger)
    {
        _zwiftToIntervalsIcuService = zwiftToIntervalsIcuService;
        _logger = logger;
    }

    [Function(nameof(WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction))]
    public async Task<ActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "whats-on-zwift-to-intervals")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string? url = req.Query["url"];
        if (url is null)
        {
            return new BadRequestObjectResult("wrong request");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return new BadRequestObjectResult("wrong url");
        }

        try
        {
            var workout = await _zwiftToIntervalsIcuService.ScrapeAndConvertToIntervalsIcu(uri, cancellationToken);

            var result = new StringBuilder();
            result.AppendLine($"{workout.FileInfo.Name}\n");
            result.AppendLine(workout.IntervalsIcuWorkoutDescription);

            return new OkObjectResult(result.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot convert WhatsOnZwift workout to Intervals.ICU workout");
            return new BadRequestObjectResult("Cannot convert WhatsOnZwift workout to Intervals.ICU workout");
        }
    }
}
