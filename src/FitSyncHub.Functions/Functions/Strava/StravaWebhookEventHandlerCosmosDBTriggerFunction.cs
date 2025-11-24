using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.Strava;

public sealed class StravaWebhookEventHandlerCosmosDBTriggerFunction
{
    private readonly StravaUpdateActivityService _updateActivityService;
    private readonly StravaSummaryActivityService _summaryActivityService;
    private readonly ILogger<StravaWebhookEventHandlerCosmosDBTriggerFunction> _logger;

    public StravaWebhookEventHandlerCosmosDBTriggerFunction(
        StravaUpdateActivityService updateActivityService,
        StravaSummaryActivityService summaryActivityService,
        ILogger<StravaWebhookEventHandlerCosmosDBTriggerFunction> logger)
    {
        _updateActivityService = updateActivityService;
        _summaryActivityService = summaryActivityService;
        _logger = logger;
    }

#if !DEBUG
    [Function(nameof(StravaWebhookEventHandlerCosmosDBTriggerFunction))]
#endif
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "fit-sync-hub",
            containerName: "WebhookEvent",
            Connection = "AzureWebJobsStorageConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<WebhookEventData> input,
        CancellationToken cancellationToken)
    {
        if (input == null || input.Count == 0)
        {
            _logger.LogInformation("Skipping cause no input from cosmos db trigger");
            return;
        }

        _logger.LogInformation("Documents modified: {Count}", input.Count);

        foreach (var webhookEventData in input.OrderBy(x => x.CreatedOn))
        {
            await HandleWebhookEventData(webhookEventData, cancellationToken);
        }
    }

    private async Task HandleWebhookEventData(WebhookEventData webhookEventData,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document for activity Id: {ActivityId}", webhookEventData.ActivityId);

        if (webhookEventData.AspectType == "delete")
        {
            await _summaryActivityService
                .DeleteSummaryActivity(webhookEventData.ActivityId, cancellationToken);
            return;
        }

        if (webhookEventData.AspectType == "create")
        {
            await _updateActivityService.UpdateActivity(webhookEventData, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Skip updating, because AspectType =! create. Aspect type: {AspectType}", webhookEventData.AspectType);
        }

        await _summaryActivityService.StoreSummaryActivity(webhookEventData.ActivityId, cancellationToken);
    }
}
