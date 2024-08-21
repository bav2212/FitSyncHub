using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.Services;

namespace StravaWebhooksAzureFunctions.Functions;

public class CosmosDBTriggerFunction
{
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly UpdateActivityService _updateActivityService;
    private readonly ILogger _logger;

    public CosmosDBTriggerFunction(
        IStravaRestHttpClient stravaRestHttpClient,
        UpdateActivityService updateActivityService,
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<CosmosDBTriggerFunction>();
        _stravaRestHttpClient = stravaRestHttpClient;
        _updateActivityService = updateActivityService;
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
            var athleteId = webhookEventData.OwnerId;
            var activityId = webhookEventData.ObjectId;

            _logger.LogInformation("Document for activity Id: {ActivityId}", activityId);

            if (webhookEventData.AspectType != "create")
            {
                _logger.LogInformation("Skip, because AspectType =! create. Aspect type: {AspectType}", webhookEventData.AspectType);
                return;
            }

            var activityResponse = await _stravaRestHttpClient
                .GetActivity(activityId, athleteId, executionContext.CancellationToken);

            await _updateActivityService.UpdateActivity(webhookEventData, activityResponse, executionContext.CancellationToken);
        }
    }
}
