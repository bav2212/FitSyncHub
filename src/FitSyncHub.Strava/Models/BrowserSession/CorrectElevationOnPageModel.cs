namespace FitSyncHub.Strava.Models.BrowserSession;

public record CorrectElevationOnPageModel
{
    public required long ActivityId { get; init; }
    public required string ActiveSource { get; init; }
    public required bool LookupExists { get; init; }
}
