namespace FitSyncHub.Strava.Models.Responses.Activities;

public record ActivityStatsVisibility
{
    public required string Type { get; init; }
    public required string Visibility { get; init; }
}
