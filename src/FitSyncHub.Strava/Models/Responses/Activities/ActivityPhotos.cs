namespace FitSyncHub.Strava.Models.Responses.Activities;

public record ActivityPhotos
{
    public object? Primary { get; init; }
    public int? Count { get; init; }
}
