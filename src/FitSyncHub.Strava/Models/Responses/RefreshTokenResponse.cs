namespace FitSyncHub.Strava.Models.Responses;

public sealed record RefreshTokenResponse
{
    public required string TokenType { get; init; }
    public required int ExpiresAt { get; init; }
    public required int ExpiresIn { get; init; }
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
}
