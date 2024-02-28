using System.Net;

namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

public record CookieLoginResponse
{
    public bool Success { get; init; }
    public CookieContainer Cookies { get; init; }
    public string AuthenticityToken { get; init; }
}
