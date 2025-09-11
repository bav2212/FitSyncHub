namespace FitSyncHub.Xert.Models;

public record TokenModel
{
    public required string AccessToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required string TokenType { get; init; }
    public required string Scope { get; init; }
    public required string RefreshToken { get; init; }
    public required long ExpiresAt { get; init; }
}
