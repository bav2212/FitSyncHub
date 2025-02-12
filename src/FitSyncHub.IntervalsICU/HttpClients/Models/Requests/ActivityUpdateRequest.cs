using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public class ActivityUpdateRequest
{
    public required ActivitySubType SubType { get; set; }
    public required int? IcuTrainingLoad { get; set; }
    public required bool? Trainer { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
    public required GearUpdateRequest Gear { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<ActivitySubType>))]
public enum ActivitySubType
{
    None,
    Commute,
    Warmup,
    Cooldown,
    Race
}

public class GearUpdateRequest
{
    public required string Id { get; set; }
}
