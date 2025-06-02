namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public record EventResponse
{
    public required int Id { get; init; }
    public required DateTime StartDateLocal { get; init; }
    public required string Type { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public bool? Indoor { get; init; }
    public required List<string>? Tags { get; init; }
    public string? PairedActivityId { get; init; }
    public long? IcuTrainingLoad { get; init; }
    public long? MovingTime { get; init; }
    public long? Joules { get; init; }
}
