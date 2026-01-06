using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.Auth;

public sealed record ZwiftAuthTokenModel
{
    public required string AccessToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required string RefreshToken { get; init; }
    public required int RefreshExpiresIn { get; init; }
    public required string TokenType { get; init; }
    [JsonPropertyName("not-before-policy")]
    public required long NotBeforePolicy { get; init; }
    public required Guid SessionState { get; init; }
    public required string Scope { get; init; }
}
