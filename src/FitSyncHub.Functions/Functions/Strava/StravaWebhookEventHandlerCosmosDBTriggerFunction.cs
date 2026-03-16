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
            databaseName: Constants.CosmosDb.DatabaseName,
            containerName: Constants.CosmosDb.Containers.StravaWebhookEvent,
            Connection = Constants.CosmosDb.ConnectionString,
            LeaseContainerName = Constants.CosmosDb.LeaseContainerName,
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<StravaWebhookEventData> input,
        CancellationToken cancellationToken)
    {
        if (input == null || input.Count == 0)
        {
            _logger.LogInformation("Skipping cause no input from cosmos db trigger");
            return;
        }

#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("Documents modified: {Count}", input.Count);
#pragma warning restore CA1873 // Avoid potentially expensive logging

        foreach (var webhookEventData in input.OrderBy(x => x.CreatedOn))
        {
            await HandleWebhookEventData(webhookEventData, cancellationToken);
        }
    }

    private async Task HandleWebhookEventData(StravaWebhookEventData webhookEventData,
        CancellationToken cancellationToken)
    {
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("Document for activity Id: {ActivityId}", webhookEventData.ActivityId);
#pragma warning restore CA1873 // Avoid potentially expensive logging

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
#pragma warning disable CA1873 // Avoid potentially expensive logging
            _logger.LogInformation("Skip updating, because AspectType =! create. Aspect type: {AspectType}",
                webhookEventData.AspectType);
#pragma warning restore CA1873 // Avoid potentially expensive logging
        }

        await _summaryActivityService.StoreSummaryActivity(webhookEventData.ActivityId, cancellationToken);
    }
}
