namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityAthlete
{
    public int? Id { get; init; }
    public string? IdStr { get; init; }
    public int? ResourceState { get; init; }
}
