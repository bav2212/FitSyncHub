namespace FitSyncHub.Functions.Options;

public record StravaOptions
{
    public const string Position = "Strava";

    public required StravaAuthOptions Auth { get; init; }
    public required StravaCredentialsOptions Credentials { get; init; }
    public required string WebhookVerifyToken { get; init; }

    public record StravaAuthOptions
    {
        public required string ClientId { get; init; }
        public required string ClientSecret { get; init; }
    }

    public record StravaCredentialsOptions
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}
