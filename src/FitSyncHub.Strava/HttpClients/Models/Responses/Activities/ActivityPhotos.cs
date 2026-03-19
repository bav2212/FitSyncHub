namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityPhotos
{
    public object? Primary { get; init; }
    public int? Count { get; init; }
}
