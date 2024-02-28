using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Extensions;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using StravaWebhooksAzureFunctions.Options;
using System.Net.Http.Json;

namespace StravaWebhooksAzureFunctions.HttpClients;

public class StravaOAuthHttpClient : IStravaOAuthHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly StravaOptions _stravaOptions;

    public StravaOAuthHttpClient(HttpClient httpClient, IOptions<StravaOptions> options)
    {
        _httpClient = httpClient;
        _stravaOptions = options.Value;
    }

    public async Task<ExchangeTokenResponse> ExchangeTokenAsync(string code, CancellationToken cancellationToken)
    {
        var request = new ExchangeTokenRequest
        {
            ClientId = _stravaOptions.Auth.ClientId,
            ClientSecret = _stravaOptions.Auth.ClientSecret,
            Code = code,
            GrantType = "authorization_code"
        };

        var content = JsonContent.Create(request);
        var response = await _httpClient.PostAsync("oauth/token", content, cancellationToken);

        return (await response.HandleJsonResponse<ExchangeTokenResponse>(cancellationToken))!;
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var request = new RefreshTokenRequest
        {
            ClientId = _stravaOptions.Auth.ClientId,
            ClientSecret = _stravaOptions.Auth.ClientSecret,
            GrantType = "refresh_token",
            RefreshToken = refreshToken
        };

        var content = JsonContent.Create(request);
        var response = await _httpClient.PostAsync("oauth/token", content, cancellationToken);

        return (await response.HandleJsonResponse<RefreshTokenResponse>(cancellationToken))!;
    }
}
