namespace StravaWebhooksAzureFunctions.Data.Entities;

public class WebhookEventData
{
#pragma warning disable IDE1006 // Naming Styles
    public required string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public required string ObjectType { get; init; }
    public required long ObjectId { get; init; }
    public required string AspectType { get; init; }
    public required string Updates { get; init; }
    public required long OwnerId { get; init; }
    public required long SubscriptionId { get; init; }
    public required long EventTime { get; init; }
    public required DateTimeOffset CreatedOn { get; init; }
}
