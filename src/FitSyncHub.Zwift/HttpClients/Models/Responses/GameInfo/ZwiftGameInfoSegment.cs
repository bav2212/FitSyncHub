namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public sealed record ZwiftGameInfoSegment
{
    public required string ArchFriendlyName { get; init; }
    public required string ArchFriendlyFemaleName { get; init; }
    public required long Id { get; init; }
    public required int ArchId { get; init; }
    public required string Direction { get; init; }
    public required int RoadId { get; init; }
    public required double RoadTime { get; init; }
    public required int World { get; init; }
    public required string JerseyName { get; init; }
    public required string JerseyIconPath { get; init; }
}

