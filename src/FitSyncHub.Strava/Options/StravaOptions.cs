namespace FitSyncHub.Strava.Options;

public sealed record StravaOptions
{
    public required StravaAuthOptions Auth { get; set; }
    public required string WebhookVerifyToken { get; set; }
    public required long AthleteId { get; set; }
    public string? CityBikeGearId { get; set; }
    public string BaseAddress { get; set; } = "http://www.strava.com";

    public sealed record StravaAuthOptions
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}
