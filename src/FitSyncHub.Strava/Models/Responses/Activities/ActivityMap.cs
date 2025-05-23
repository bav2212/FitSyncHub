namespace FitSyncHub.Strava.Models.Responses.Activities;

public record ActivityMap
{
    public string? Id { get; init; }
    public string? Polyline { get; init; }
    public int? ResourceState { get; init; }
    public string? SummaryPolyline { get; init; }
}
