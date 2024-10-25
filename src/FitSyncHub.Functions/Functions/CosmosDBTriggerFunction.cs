using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class CosmosDBTriggerFunction
{
    private readonly UpdateActivityService _updateActivityService;
    private readonly StoreSummaryActivitiesService _storeActivitiesService;
    private readonly ILogger _logger;

    public CosmosDBTriggerFunction(
        UpdateActivityService updateActivityService,
        StoreSummaryActivitiesService storeActivitiesService,
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<CosmosDBTriggerFunction>();
        _updateActivityService = updateActivityService;
        _storeActivitiesService = storeActivitiesService;
    }

    [Function(nameof(CosmosDBTriggerFunction))]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "strava",
            containerName: "WebhookEvent",
            Connection = "AzureWebJobsStorageConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<WebhookEventData> input,
        FunctionContext executionContext)
    {
        if (input == null || input.Count <= 0)
        {
            _logger.LogInformation("Skipping cause no input from cosmos db trigger");
            return;
        }

        _logger.LogInformation("Documents modified: {Count}", input.Count);
        foreach (var webhookEventData in input)
        {
            await HandleWebhookEventData(webhookEventData, executionContext.CancellationToken);
        }
    }

    private async Task HandleWebhookEventData(WebhookEventData webhookEventData, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document for activity Id: {ActivityId}", webhookEventData.ActivityId);

        if (webhookEventData.AspectType == "create")
        {
            await _updateActivityService.UpdateActivity(webhookEventData, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Skip updating, because AspectType =! create. Aspect type: {AspectType}", webhookEventData.AspectType);
        }

        await _storeActivitiesService
            .StoreSummaryActivity(webhookEventData.AthleteId, webhookEventData.ActivityId, cancellationToken);
    }
}
