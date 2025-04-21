namespace FitSyncHub.Strava.Models.Requests;

public record UpdatableActivityRequest
{
    public required bool Commute { get; init; }
    public required bool Trainer { get; init; }
    public required bool? HideFromHome { get; init; }
    public required string? Description { get; init; }
    public required string Name { get; init; }
    public required SportType SportType { get; init; }
    public required string? GearId { get; init; }
}
