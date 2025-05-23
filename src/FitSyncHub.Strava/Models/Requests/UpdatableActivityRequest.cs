namespace FitSyncHub.Strava.Models.Requests;

public record UpdatableActivityRequest
{
    public bool? Commute { get; init; }
    public bool? Trainer { get; init; }
    public bool? HideFromHome { get; init; }
    public string? Description { get; init; }
    public string? Name { get; init; }
    public SportType? SportType { get; init; }
    public string? GearId { get; init; }
}
