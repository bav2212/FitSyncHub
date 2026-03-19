namespace FitSyncHub.Strava.Models.Requests;

public sealed record UpdateAthleteRequest
{
    public required float Weight { get; init; }
}
