using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

public class RefreshTokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }
    [JsonPropertyName("expires_at")]
    public int ExpiresAt { get; init; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }
}
