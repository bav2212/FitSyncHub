using System.Net;
using System.Net.Http.Json;
using FitSyncHub.Common.Abstractions;
using FitSyncHub.Common.Exceptions;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Strava.HttpClients;

public class StravaOAuthHttpClient : IStravaOAuthHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IStravaApplicationOptionsProvider _stravaApplicationOptionsProvider;

    public StravaOAuthHttpClient(
        HttpClient httpClient,
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

        var content = JsonContent.Create(request, StravaAuthHttpClientSerializerContext.Default.ExchangeTokenRequest);
        var response = await _httpClient.PostAsync("oauth/token", content, cancellationToken);
        EnsureSuccessOrThrowNotFound(response);

        return await response.Content.ReadFromJsonAsync(
            StravaAuthHttpClientSerializerContext.Default.ExchangeTokenResponse,
            cancellationToken) ?? throw new InvalidDataException();
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

        var content = JsonContent.Create(request, StravaAuthHttpClientSerializerContext.Default.RefreshTokenRequest);
        var response = await _httpClient.PostAsync("oauth/token", content, cancellationToken);
        EnsureSuccessOrThrowNotFound(response);

        return await response.Content.ReadFromJsonAsync(
            StravaAuthHttpClientSerializerContext.Default.RefreshTokenResponse,
            cancellationToken) ?? throw new InvalidDataException();
    }

    private static void EnsureSuccessOrThrowNotFound(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException("Entity not found");
        }

        // raise exception if response is not success
        response.EnsureSuccessStatusCode();
    }
}
