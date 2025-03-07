namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public record EventResponse
{
    public required int Id { get; init; }
    public required string StartDateLocal { get; init; }
    public required string Type { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required bool Indoor { get; init; }
    public required List<string>? Tags { get; init; }
}
