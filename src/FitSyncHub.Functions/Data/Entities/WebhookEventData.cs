using System.Text.Json.Serialization;
using FitSyncHub.Functions.Data.Entities.Abstractions;

namespace FitSyncHub.Functions.Data.Entities;

public sealed class WebhookEventData : DataModel
{
    public required string ObjectType { get; init; }
    public required long ObjectId { get; init; }
    public required string AspectType { get; init; }
    public required string Updates { get; init; }
    public required long OwnerId { get; init; }
    public required long SubscriptionId { get; init; }
    public required long EventTime { get; init; }
    public required DateTimeOffset CreatedOn { get; init; }

    [JsonIgnore]
    public long AthleteId => OwnerId;
    [JsonIgnore]
    public long ActivityId => ObjectId;
}
