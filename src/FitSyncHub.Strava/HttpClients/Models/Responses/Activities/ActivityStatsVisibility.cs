namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityStatsVisibility
{
    public required string Type { get; init; }
    public required string Visibility { get; init; }
}
