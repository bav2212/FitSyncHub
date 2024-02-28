using System;

namespace StravaWebhooksAzureFunctions.Data.Entities;

public class WebhookEventData
{
    public long Id { get; init; }
    public string ObjectType { get; init; }
    public long ObjectId { get; init; }
    public string AspectType { get; init; }
    public string Updates { get; init; }
    public long OwnerId { get; init; }
    public long SubscriptionId { get; init; }
    public long EventTime { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}
