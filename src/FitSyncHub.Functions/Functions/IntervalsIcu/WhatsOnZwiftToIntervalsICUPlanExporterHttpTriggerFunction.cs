using System.Runtime.CompilerServices;
using FitSyncHub.IntervalsICU.Models;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace FitSyncHub.Functions.Functions.IntervalsIcu;

public sealed class WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction
{
    private readonly WhatsOnZwiftToIntervalsIcuService _zwiftToIntervalsIcuService;
    private readonly WhatsOnZwiftScraperService _whatsOnZwiftScraper;
    private readonly IntervalsIcuStorageService _intervalsIcuStorageService;
    private readonly ILogger<WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction> _logger;

    public WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction(
        WhatsOnZwiftToIntervalsIcuService zwiftToIntervalsIcuService,
        WhatsOnZwiftScraperService whatsOnZwiftScraper,
        IntervalsIcuStorageService intervalsIcuStorageService,
        ILogger<WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction> logger)
    {
        _zwiftToIntervalsIcuService = zwiftToIntervalsIcuService;
        _whatsOnZwiftScraper = whatsOnZwiftScraper;
        _intervalsIcuStorageService = intervalsIcuStorageService;
        _logger = logger;
    }

    [Function(nameof(WhatsOnZwiftToIntervalsICUPlanExporterHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "whats-on-zwift-to-intervals-plan-exporter")] HttpRequest req,
        [FromBody] IntervalICUPlanExporterRequest request,
        CancellationToken cancellationToken)
    {
        _ = req;
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        if (!Uri.TryCreate(request.PlanUrl, UriKind.Absolute, out var planUri))
        {
            return new BadRequestObjectResult("wrong url");
        }

        try
        {
            var links = await _whatsOnZwiftScraper.ScrapeWorkoutPlanLinks(planUri, cancellationToken);
            var items = await ScrapeWorkoutsAndConvertToIntervalsIcu(links, cancellationToken)
                .ToListAsync(cancellationToken);

            await _intervalsIcuStorageService.Store(items, request.FolderId, cancellationToken);
            return new OkObjectResult("Stored plan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot convert and store WhatsOnZwift plan to Intervals.ICU plan");
            return new BadRequestObjectResult("Cannot convert and store WhatsOnZwift plan to Intervals.ICU plan");
        }
    }

    private async IAsyncEnumerable<WhatsOnZwiftToIntervalsIcuConvertResult> ScrapeWorkoutsAndConvertToIntervalsIcu(
        List<string> links,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var link in links)
        {
            var linkUri = new Uri(link);
            yield return await _zwiftToIntervalsIcuService.ScrapeAndConvertToIntervalsIcu(linkUri, cancellationToken);
        }
    }
}

public sealed record IntervalICUPlanExporterRequest
{
    public required string PlanUrl { get; init; }
    public required int FolderId { get; init; }
}
