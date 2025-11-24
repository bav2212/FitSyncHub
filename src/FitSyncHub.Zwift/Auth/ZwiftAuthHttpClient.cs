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
    private readonly IOptions<ZwiftAuthOptions> _authOptions;
    private readonly ILogger<ZwiftAuthHttpClient> _logger;

    public ZwiftAuthHttpClient(
        HttpClient httpClient,
        IOptions<ZwiftAuthOptions> authOptions,
        ILogger<ZwiftAuthHttpClient> logger)
    {
        _httpClient = httpClient;
        _authOptions = authOptions;
        _logger = logger;
    }

    public async Task<ZwiftAuthToken> Authenticate(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting authentication process...");

        const string Url = "/auth/realms/zwift/tokens/access/codes";
        var formParamets = new Dictionary<string, string>
        {
            { "username", _authOptions.Value.Username },
            { "password", _authOptions.Value.Password },
            { "grant_type", "password" },
            { "client_id", "Zwift_Mobile_Link" }
        };

        var formContent = new FormUrlEncodedContent(formParamets);
        var response = await _httpClient.PostAsync(Url, formContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var token = JsonSerializer.Deserialize(content, ZwiftAuthGenerationContext.Default.ZwiftAuthToken)!;
        _logger.LogInformation("OAuth2 token: {AccessToken}", token.AccessToken);

        _logger.LogInformation("Authentication process completed.");
        return token;
    }

    public async Task<ZwiftAuthToken> RefreshToken(ZwiftAuthToken token, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting token refresh with refresh token: {Token}", token.RefreshToken);

        const string Url = "/auth/realms/zwift/tokens/access/codes";
        var formParamets = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", token.RefreshToken },
        };

        var formContent = new FormUrlEncodedContent(formParamets);
        var response = await _httpClient.PostAsync(Url, formContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var newToken = JsonSerializer.Deserialize(content, ZwiftAuthGenerationContext.Default.ZwiftAuthToken)!;
        _logger.LogInformation("OAuth2 token: {AccessToken}", newToken.AccessToken);

        _logger.LogInformation("Refresh process completed.");
        return newToken;
    }
}
