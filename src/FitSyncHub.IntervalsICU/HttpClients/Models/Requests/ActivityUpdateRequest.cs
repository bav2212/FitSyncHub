using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record ActivityUpdateRequest
{
    public ActivitySubType? SubType { get; init; }
    public int? IcuTrainingLoad { get; init; }
    public bool? Trainer { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Type { get; init; }
    public GearUpdateRequest? Gear { get; init; }
    public int? IcuRpe { get; init; }
    public int? Feel { get; init; }
}

public record GearUpdateRequest
{
    public required string Id { get; init; }
}
