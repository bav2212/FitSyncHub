namespace FitSyncHub.GarminConnect.Auth.Models;

public record GarminOAuth1Token
{
    public required string Token { get; init; }
    public required string TokenSecret { get; init; }
    public required string? MfaToken { get; init; }
    public required string? MfaExpirationTimestamp { get; init; }
}
