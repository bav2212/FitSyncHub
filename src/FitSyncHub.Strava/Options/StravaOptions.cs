namespace FitSyncHub.Strava.Options;

public record StravaOptions
{
    public const string Position = "Strava";

    public required StravaAuthOptions Auth { get; init; }
    public required string WebhookVerifyToken { get; init; }
    public required long AthleteId { get; init; }
    public string? CityBikeGearId { get; init; }

    public record StravaAuthOptions
    {
        public required string ClientId { get; init; }
        public required string ClientSecret { get; init; }
    }
}
