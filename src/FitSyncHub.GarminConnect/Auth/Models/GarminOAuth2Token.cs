namespace FitSyncHub.GarminConnect.Auth.Models;

public record GarminOAuth2Token
{
    public required string Scope { get; init; }
    public required string Jti { get; init; }
    public required string TokenType { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required int RefreshTokenExpiresIn { get; init; }
}
