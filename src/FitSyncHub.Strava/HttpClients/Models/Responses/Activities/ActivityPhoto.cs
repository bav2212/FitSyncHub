namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityPhoto
{
    public string? UniqueId { get; init; }
    public Dictionary<string,string>? Urls { get; init; }
    public int? Source { get; init; }
    public int? MediaType { get; init; }
}
