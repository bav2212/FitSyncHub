namespace FitSyncHub.GarminConnect.Models.Responses;

public record GarminActivityResponse
{
    public required long ActivityId { get; init; }
    public required string ActivityName { get; init; }
    public string? Description { get; init; }
    public required ActivityType ActivityType { get; init; }
    public required EventType EventType { get; init; }
    public required double Distance { get; init; }
    public required double Duration { get; init; }
    public required double ElapsedDuration { get; init; }
    public required double MovingDuration { get; init; }
    public double? ElevationGain { get; init; }
    public double? ElevationLoss { get; init; }
    public required double AverageSpeed { get; init; }
}

public record ActivityType
{
    public required long TypeId { get; init; }
    public required string TypeKey { get; init; }
    public required long ParentTypeId { get; init; }
    public required bool IsHidden { get; init; }
    public required bool Restricted { get; init; }
    public required bool Trimmable { get; init; }
}

public record EventType
{
    public required long TypeId { get; init; }
    public required string TypeKey { get; init; }
    public required long SortOrder { get; init; }
}
