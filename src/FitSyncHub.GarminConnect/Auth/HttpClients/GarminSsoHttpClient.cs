using System.Net;
using System.Text.RegularExpressions;
using FitSyncHub.GarminConnect.Auth.Exceptions;
using FitSyncHub.GarminConnect.Auth.Models;
using FitSyncHub.GarminConnect.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.GarminConnect.Auth;

internal partial class GarminSsoHttpClient
{
    private readonly HttpClient _ssoClient;
    private readonly ILogger<GarminSsoHttpClient> _logger;
    private readonly GarminConnectAuthOptions _authOptions;
    private readonly string _embedUrl;
    private readonly Dictionary<string, string> _embedParams;
    private readonly Dictionary<string, string> _signInParams;
    private readonly string _signInUrl;
    private readonly string _verifyMFA;

    public GarminSsoHttpClient(
        IHttpClientFactory httpClientFactory,
        IOptions<GarminConnectAuthOptions> authOptions,
        ILogger<GarminSsoHttpClient> logger)
    {
        _ssoClient = httpClientFactory.CreateClient("GarminSSOClient");

        _authOptions = authOptions.Value;
        _logger = logger;

        var ssoUrl = _ssoClient.BaseAddress!.ToString()!;

        _embedUrl = $"{ssoUrl}/embed";
        _embedParams = new()
        {
            { "id", "gauth-widget" },
            { "embedWidget", "true" },
            { "gauthHost", ssoUrl }
        };

        _signInParams = new()
        {
            { "id", "gauth-widget" },
            { "embedWidget", "true" },
            { "gauthHost", _embedUrl },
            { "service", _embedUrl },
            { "source", _embedUrl },
            { "redirectAfterAccountLoginUrl", _embedUrl },
            { "redirectAfterAccountCreationUrl", _embedUrl },
        };

        _signInUrl = $"{ssoUrl}/signin";
        _verifyMFA = $"{ssoUrl}/verifyMFA/loginEnterMfaCode";
    }

    public async Task<string> Login(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting authentication process...");

        await SetCookies(cancellationToken);
        _logger.LogInformation("Cookies retrieved");

        var csrf = await RequestCsrfToken(cancellationToken);
        _logger.LogInformation("CSRF token retrieved: {Csrf}", csrf);

        var ticket = await GetOAuthTicket(csrf, cancellationToken);
        _logger.LogInformation("OAuth1 ticket: {Ticket}", ticket);

        return ticket;
    }

    public async Task<string> ResumeLogin(string mfaCode,
       GarminNeedsMfaClientState needMfaState,
       CancellationToken cancellationToken)
    {
        var query = QueryString.Create(_signInParams);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_verifyMFA}{query}");

        httpRequestMessage.Headers.Add("referer", _verifyMFA);
        httpRequestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "mfa-code", mfaCode },
            { "embed", "true" },
            { "_csrf", needMfaState.Csrf },
            { "fromPage", "setupEnterMfaCode" }
        });

        var response = await _ssoClient.SendAsync(httpRequestMessage, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var title = GetTitle(content);
        if (title != "Success")
        {
            throw new GarminConnectAuthenticationException("Unexpected error while mfa verification");
        }

        return ParseOAuthTicket(content);
    }

    private async Task SetCookies(CancellationToken cancellationToken)
    {
        var query = QueryString.Create(_embedParams);

        var url = $"{_embedUrl}{query}";

        var response = await _ssoClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> RequestCsrfToken(CancellationToken cancellationToken)
    {
        var queryParams = QueryString.Create(_signInParams);
        var url = $"{_signInUrl}{queryParams}";

        var response = await _ssoClient.GetAsync(url, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new GarminConnectAuthenticationException("Failed to fetch csrf token from Garmin.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var match = RegexCSRF().Match(content);
        if (!match.Success)
        {
            throw new GarminConnectAuthenticationException("Failed to find regex match for csrf token.");
        }

        return match.Groups[1].Value;
    }

    private async Task<string> GetOAuthTicket(string csrf, CancellationToken cancellationToken)
    {
        var query = QueryString.Create(_signInParams);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_signInUrl}{query}");

        httpRequestMessage.Headers.Add("referer", _signInUrl);
        httpRequestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "embed", "true" },
            { "username", _authOptions.Username },
            { "password", _authOptions.Password },
            { "_csrf", csrf },
        });

        var response = await _ssoClient.SendAsync(httpRequestMessage, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var title = GetTitle(content);
        if (title.Contains("MFA"))
        {
            var needsMfaState = new GarminNeedsMfaClientState
            {
                Csrf = csrf,
            };
            throw new GarminConnectNeedsMfaException(needsMfaState);
        }

        return ParseOAuthTicket(content);
    }

    private static string ParseOAuthTicket(string content)
    {
        var match = RegexTicket().Match(content);
        if (!match.Success)
        {
            throw new GarminConnectAuthenticationException("Failed to find regex match for ticket.");
        }

        return match.Groups[1].Value;
    }

    private static string GetTitle(string content)
    {
        var title = RegexTitle().Match(content);
        return !title.Success
            ? throw new Exception("Couldn't find title")
            : title.Groups[1].Value;
    }

    [GeneratedRegex("name=\"_csrf\"\\s+value=\"(.+?)\"", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex RegexCSRF();

    [GeneratedRegex("<title>(.+?)</title>")]
    private static partial Regex RegexTitle();

    [GeneratedRegex(@"embed\?ticket=([^""]+)""", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex RegexTicket();
}
