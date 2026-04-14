using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using FitSyncHub.GarminConnect.Auth.Exceptions;
using FitSyncHub.GarminConnect.Auth.Models;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.GarminConnect.Auth.HttpClients;

internal sealed class GarminDiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GarminDiHttpClient> _logger;

    private const string Di_TOKEN_URL = "https://diauth.garmin.com/di-oauth2-service/oauth/token";

    public GarminDiHttpClient(HttpClient httpClient,
        ILogger<GarminDiHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GarminDiTokenModel> Refresh(GarminDiTokenModel tokenModel,
        CancellationToken cancellationToken)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Di_TOKEN_URL);

        httpRequestMessage.Headers.Add("Authorization", BuildBasicAuth(tokenModel.DiClientId));
        httpRequestMessage.Headers.Add("Accept", "application/json");
        httpRequestMessage.Headers.Add("Cache-Control", "no-cache");

        httpRequestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", tokenModel.DiClientId },
            { "refresh_token", tokenModel.DiRefreshToken },
        });

        var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cannot exchange refresh token for access token. Response: {Content}", content);
            throw new GarminConnectAuthenticationException("Cannot exchange refresh token for access token.");
        }

        var data = JsonSerializer.Deserialize(content, GarmimDiHttpClientSerializerContext.Default.GarminDiRefreshTokenResponse)!;

        var aceessToken = data.AccessToken;
        var refreshToken = data.RefreshToken;
        var clientId = ExtractClientIdFromJwt(aceessToken) ?? tokenModel.DiClientId;

        return new GarminDiTokenModel
        {
            DiToken = aceessToken,
            DiRefreshToken = refreshToken,
            DiClientId = clientId
        };
    }

    private static string? ExtractClientIdFromJwt(string aceessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(aceessToken);

        return jwtSecurityToken.Claims
           .FirstOrDefault(c => c.Type == "client_id")
           ?.Value;
    }

    private static string BuildBasicAuth(string clientId)
    {
        return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:"));
    }
}
