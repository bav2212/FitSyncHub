namespace StravaWebhooksAzureFunctions.HttpClients.Models.Requests;

public record RefreshTokenRequest
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string GrantType { get; init; }
    public required string RefreshToken { get; init; }
}
