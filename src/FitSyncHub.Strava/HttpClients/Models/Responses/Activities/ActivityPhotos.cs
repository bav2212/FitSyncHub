namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityPhotos
{
    public ActivityPhoto? Primary { get; init; }
    public bool? UsePrimaryPhoto { get; init; }
    public int? Count { get; init; }
}
