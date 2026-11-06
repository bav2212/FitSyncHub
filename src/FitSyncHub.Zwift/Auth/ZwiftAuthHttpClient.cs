using System.Text.Json;
using FitSyncHub.Zwift.Auth.Abstractions;
using FitSyncHub.Zwift.JsonSerializerContexts;
using FitSyncHub.Zwift.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Zwift.Auth;

internal sealed class ZwiftAuthHttpClient : IZwiftAuthenticator, IZwiftTokenRefresher
{
    private readonly HttpClient _httpClient;
    private readonly ZwiftOptions _zwiftOptions;
    private readonly ILogger<ZwiftAuthHttpClient> _logger;
    private const string Url = "/auth/realms/zwift/protocol/openid-connect/token";

    public ZwiftAuthHttpClient(
        IHttpClientFactory httpClientFactory,
        IOptions<ZwiftOptions> zwiftOptions,
        ILogger<ZwiftAuthHttpClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.ZwiftAuthHttpClient);
        _zwiftOptions = zwiftOptions.Value;
        _logger = logger;
    }

    public async Task<ZwiftAuthTokenModel> Authenticate(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting authentication process...");

        var formParamets = new Dictionary<string, string>
        {
            { "client_id", "Zwift Game Client" },
            { "grant_type", "password" },
            { "username", _zwiftOptions.Credentials.Username },
            { "password", _zwiftOptions.Credentials.Password },
        };

        var formContent = new FormUrlEncodedContent(formParamets);
        var response = await _httpClient.PostAsync(Url, formContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var token = JsonSerializer.Deserialize(
            content, ZwiftAuthHttpClientGenerationContext.Default.ZwiftAuthTokenModel)!;
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("OAuth2 token: {AccessToken}", token.AccessToken);
#pragma warning restore CA1873 // Avoid potentially expensive logging

        _logger.LogInformation("Authentication process completed.");
        return token;
    }

    public async Task<ZwiftAuthTokenModel> RefreshToken(ZwiftAuthTokenModel token, CancellationToken cancellationToken)
    {
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("Starting token refresh with refresh token: {Token}", token.RefreshToken);
#pragma warning restore CA1873 // Avoid potentially expensive logging

        var formParamets = new Dictionary<string, string>
        {
            { "client_id", "Zwift Game Client" },
            { "grant_type", "refresh_token" },
            { "refresh_token", token.RefreshToken },
        };

        var formContent = new FormUrlEncodedContent(formParamets);
        var response = await _httpClient.PostAsync(Url, formContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var newToken = JsonSerializer.Deserialize(
            content, ZwiftAuthHttpClientGenerationContext.Default.ZwiftAuthTokenModel)!;
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("OAuth2 token: {AccessToken}", newToken.AccessToken);
#pragma warning restore CA1873 // Avoid potentially expensive logging

        _logger.LogInformation("Refresh process completed.");
        return newToken;
    }
}
