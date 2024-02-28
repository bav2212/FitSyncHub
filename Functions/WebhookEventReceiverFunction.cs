using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.Options;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace StravaWebhooksAzureFunctions.Functions;

public class WebhookEventReceiverFunction
{
    private readonly StravaOptions _stravaOptions;

    public WebhookEventReceiverFunction(IOptions<StravaOptions> stravaOptions)
    {
        _stravaOptions = stravaOptions.Value;
    }

    [Function(nameof(WebhookEventReceiverFunction))]
    public static async Task<WebhookEventReceiverMultiResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook")] HttpRequestData req,
        [FromBody] WebhookEventDataWebhookRequest request,
        FunctionContext executionContext)
    {
        if (request.OwnerId != Constants.MyAthleteId)
        {
            return new WebhookEventReceiverMultiResponse
            {
                Document = default,
                HttpResponse = req.CreateResponse(HttpStatusCode.BadRequest)
            };
        }

        var webhookEventData = new WebhookEventData
        {
            id = request.ObjectId.ToString(),
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
            HttpResponse = req.CreateResponse(HttpStatusCode.OK)
        };
    }


    private static async Task<string> ReadRequestBody(HttpRequest req)
    {
        using StreamReader streamReader = new(req.Body);
        return await streamReader.ReadToEndAsync();
    }

    public record WebhookEventDataWebhookRequest
    {
        [JsonPropertyName("object_type")]
        public string ObjectType { get; init; }

        [JsonPropertyName("object_id")]
        public long ObjectId { get; init; }

        [JsonPropertyName("aspect_type")]
        public string AspectType { get; init; }

        [JsonPropertyName("updates")]
        public JsonDocument Updates { get; init; }

        [JsonPropertyName("owner_id")]
        public long OwnerId { get; init; }

        [JsonPropertyName("subscription_id")]
        public long SubscriptionId { get; init; }

        [JsonPropertyName("event_time")]
        public long EventTime { get; init; }
    }
}

public record WebhookEventReceiverMultiResponse
{
    [CosmosDBOutput(
        databaseName: "strava",
        containerName: "WebhookEvent",
        Connection = "AzureWebJobsStorageConnectionString",
        CreateIfNotExists = true,
        PartitionKey = "/id")]
    public required WebhookEventData Document { get; init; }
    public required HttpResponseData HttpResponse { get; init; }
}
