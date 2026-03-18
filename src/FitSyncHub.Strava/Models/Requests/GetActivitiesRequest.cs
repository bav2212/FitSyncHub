namespace FitSyncHub.Strava.Models.Requests;

public sealed record GetActivitiesRequest
{
    public required long Before { get; init; }
    public required long After { get; init; }
    public int Page { get; init; } = Constants.Api.AthleteActivitiesFirstPage;
    public int PerPage { get; init; } = Constants.Api.AthleteActivitiesMaxPerPage;
}
