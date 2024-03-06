using System.Net;

namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

public record CookieLoginResponse
{
    public required bool Success { get; init; }
    public required CookieContainer Cookies { get; init; }
    public required string AuthenticityToken { get; init; }
}
