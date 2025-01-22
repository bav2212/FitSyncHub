using System.Text.Json;
using System.Text.Json.Serialization;
using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Options;
using FitSyncHub.Strava;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace FitSyncHub.Functions.Functions;

public class WebhookEventReceiverFunction
{
    private readonly StravaOptions _stravaOptions;

    public WebhookEventReceiverFunction(IOptions<StravaOptions> stravaOptions)
    {
        _stravaOptions = stravaOptions.Value;
    }

    [Function(nameof(WebhookEventReceiverFunction))]
    public static WebhookEventReceiverMultiResponse Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook")] HttpRequest req,
        [FromBody] WebhookEventDataWebhookRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OwnerId != Constants.MyAthleteId)
        {
            return new WebhookEventReceiverMultiResponse
            {
                Document = default,
                Result = new BadRequestResult()
            };
        }

        var webhookEventData = new WebhookEventData
        {
            id = Guid.NewGuid().ToString(),
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

    public record WebhookEventDataWebhookRequest
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

public record WebhookEventReceiverMultiResponse
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
