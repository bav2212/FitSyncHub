using System.Net.Http.Json;
using FitSyncHub.Common;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Extensions;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Strava.HttpClients;

public class StravaOAuthHttpClient : IStravaOAuthHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IStravaApplicationOptionsProvider _stravaApplicationOptionsProvider;

    public StravaOAuthHttpClient(HttpClient httpClient,
        IStravaApplicationOptionsProvider stravaApplicationOptionsProvider)
    {
        _httpClient = httpClient;
        _stravaApplicationOptionsProvider = stravaApplicationOptionsProvider;
    }

    public async Task<ExchangeTokenResponse> ExchangeTokenAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var request = new ExchangeTokenRequest
        {
            ClientId = _stravaApplicationOptionsProvider.ClientId,
            ClientSecret = _stravaApplicationOptionsProvider.ClientSecret,
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
            ClientId = _stravaApplicationOptionsProvider.ClientId,
            ClientSecret = _stravaApplicationOptionsProvider.ClientSecret,
            GrantType = "refresh_token",
            RefreshToken = refreshToken
        };

        var content = JsonContent.Create(request, StravaRestApiSerializerContext.Default.RefreshTokenRequest);
        var response = await _httpClient.PostAsync("oauth/token", content, cancellationToken);

        return await response.HandleJsonResponse(StravaRestApiSerializerContext.Default.RefreshTokenResponse, cancellationToken);
    }
}
