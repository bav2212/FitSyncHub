using System.Net.Http.Json;
using FitSyncHub.Functions.Extensions;
using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.HttpClients.Models.Requests;
using FitSyncHub.Functions.HttpClients.Models.Responses;
using FitSyncHub.Functions.Options;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions;

namespace FitSyncHub.Functions.HttpClients;

public class StravaOAuthHttpClient : IStravaOAuthHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly StravaOptions _stravaOptions;

    public StravaOAuthHttpClient(HttpClient httpClient,
        IOptions<StravaOptions> options)
    {
        _httpClient = httpClient;
        _stravaOptions = options.Value;
    }

    public async Task<ExchangeTokenResponse> ExchangeTokenAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var request = new ExchangeTokenRequest
        {
            ClientId = _stravaOptions.Auth.ClientId,
            ClientSecret = _stravaOptions.Auth.ClientSecret,
            Code = code,
            GrantType = "authorization_code"
        };

        var content = JsonContent.Create(request, StravaRestApiSerializerContext.Default.ExchangeTokenRequest);
        var response = await _httpClient.PostAsync("oauth/token", content, cancellationToken);
        return await response.HandleJsonResponse(StravaRestApiSerializerContext.Default.ExchangeTokenResponse, cancellationToken);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var request = new RefreshTokenRequest
        {
            ClientId = _stravaOptions.Auth.ClientId,
            ClientSecret = _stravaOptions.Auth.ClientSecret,
            GrantType = "refresh_token",
            RefreshToken = refreshToken
        };

        var content = JsonContent.Create(request, StravaRestApiSerializerContext.Default.RefreshTokenRequest);
        var response = await _httpClient.PostAsync("oauth/token", content, cancellationToken);

        return await response.HandleJsonResponse(StravaRestApiSerializerContext.Default.RefreshTokenResponse, cancellationToken);
    }
}
