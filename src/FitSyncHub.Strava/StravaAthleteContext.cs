namespace FitSyncHub.Strava;

public record StravaAthleteContext
{
    public long AthleteId { get; set; } = Constants.MyAthleteId;
}
