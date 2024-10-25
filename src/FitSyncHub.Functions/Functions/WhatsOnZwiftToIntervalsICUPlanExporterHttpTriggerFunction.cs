using System.Net;
using FitSyncHub.Functions.Extensions;
using FitSyncHub.IntervalsICU.Scrapers;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction
{
    private readonly ZwiftToIntervalsIcuService _zwiftToIntervalsIcuService;
    private readonly IntervalsIcuStorageService _intervalsIcuStorageService;

    public WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction(
        ZwiftToIntervalsIcuService zwiftToIntervalsIcuService,
        IntervalsIcuStorageService intervalsIcuStorageService)
    {
        _zwiftToIntervalsIcuService = zwiftToIntervalsIcuService;
        _intervalsIcuStorageService = intervalsIcuStorageService;
    }

    [Function(nameof(WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "whats-on-zwift-to-intervals-plan-exporter")] HttpRequestData req,
        [FromBody] IntervalICUPlanExporterRequest request,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            var links = await WhatsOnZwiftScraper.ScrapeWorkoutPlanLinks(request.PlanUrl);
            List<ZwiftToIntervalsIcuConvertResult> items = [];
            foreach (var link in links)
            {
                var result = await _zwiftToIntervalsIcuService.ScrapeAndConvertToIntervalsIcu(link);
                items.Add(result);
            }

            await _intervalsIcuStorageService.Store(items, request.FolderId, executionContext.CancellationToken);

            var response = req.CreateResponse(HttpStatusCode.OK);
            logger.LogInformation("Stored plan");
            await response.WriteStringAsync("Stored plan");

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot convert and store WhatsOnZwift plan to Intervals.ICU plan");
            return req.CreateBadRequest("Cannot convert and store WhatsOnZwift plan to Intervals.ICU plan");
        }
    }
}

public record IntervalICUPlanExporterRequest
{
    public required string PlanUrl { get; init; }
    public required int FolderId { get; init; }
}
