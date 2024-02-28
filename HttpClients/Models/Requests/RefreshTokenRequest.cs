using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions.HttpClients.Models.Requests;

public record RefreshTokenRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; init; }

    [JsonPropertyName("grant_type")]
    public string GrantType { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }
}
