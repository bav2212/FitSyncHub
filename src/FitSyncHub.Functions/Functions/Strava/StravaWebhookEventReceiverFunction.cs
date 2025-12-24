using System.Text.Json;
using System.Text.Json.Serialization;
using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Strava.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace FitSyncHub.Functions.Functions.Strava;

public sealed class StravaWebhookEventReceiverFunction
{
    private readonly long _athleteId;

    public StravaWebhookEventReceiverFunction(IOptions<StravaOptions> options)
    {
        _athleteId = options.Value.AthleteId;
    }

    [Function(nameof(StravaWebhookEventReceiverFunction))]
    public WebhookEventReceiverMultiResponse Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "strava/webhook")] HttpRequest req,
        [FromBody] WebhookEventDataWebhookRequest request)
    {
        _ = req;

        if (request.OwnerId != _athleteId)
        {
            return new WebhookEventReceiverMultiResponse
            {
                Document = default,
                Result = new BadRequestResult()
            };
        }

        var webhookEventData = new WebhookEventData
        {
            Id = Guid.NewGuid().ToString(),
            ObjectType = request.ObjectType,
            ObjectId = request.ObjectId,
            AspectType = request.AspectType,
            Updates = request.Updates.RootElement.GetRawText(),
            OwnerId = request.OwnerId,
            SubscriptionId = request.SubscriptionId,
            EventTime = request.EventTime,
            CreatedOn = DateTimeOffset.UtcNow,
        };

        return new WebhookEventReceiverMultiResponse
        {
            Document = webhookEventData,
            Result = new OkResult()
        };
    }

    public sealed record WebhookEventDataWebhookRequest
    {
        [JsonPropertyName("object_type")]
        public required string ObjectType { get; init; }

        [JsonPropertyName("object_id")]
        public required long ObjectId { get; init; }

        [JsonPropertyName("aspect_type")]
        public required string AspectType { get; init; }

        [JsonPropertyName("updates")]
        public required JsonDocument Updates { get; init; }

        [JsonPropertyName("owner_id")]
        public required long OwnerId { get; init; }

        [JsonPropertyName("subscription_id")]
        public required long SubscriptionId { get; init; }

        [JsonPropertyName("event_time")]
        public required long EventTime { get; init; }
    }
}

public sealed record WebhookEventReceiverMultiResponse
{
    [CosmosDBOutput(
        databaseName: "fit-sync-hub",
        containerName: "WebhookEvent",
        Connection = "AzureWebJobsStorageConnectionString",
        CreateIfNotExists = true,
        PartitionKey = "/id")]
    public required WebhookEventData? Document { get; init; }
    [HttpResult]
    public required IActionResult Result { get; init; }
}
