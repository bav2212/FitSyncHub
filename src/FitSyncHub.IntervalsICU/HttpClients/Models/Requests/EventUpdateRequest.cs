using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record EventUpdateRequest
{
    public ActivitySubType? SubType { get; init; }
    public List<string>? Tags { get; init; }
    public uint? IcuTrainingLoad { get; init; }
    public bool? Trainer { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Type { get; init; }
    public GearUpdateRequest? Gear { get; init; }
    public uint? IcuRpe { get; init; }
    public uint? Feel { get; init; }
    public uint? IcuFtp { get; init; }
}
