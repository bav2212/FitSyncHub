namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityReference
{
    public long? Id { get; init; }
    public string? IdStr { get; init; }
    public string? Visibility { get; init; }
    public int? ResourceState { get; init; }
}
