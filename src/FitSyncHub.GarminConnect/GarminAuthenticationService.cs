using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Auth;
using Garmin.Connect.Auth;
using Garmin.Connect.Auth.External;
using OAuth;
using ConsumerCredentials = FitSyncHub.GarminConnect.Models.Auth.ConsumerCredentials;

namespace FitSyncHub.GarminConnect;

internal class GarminAuthenticationService : IGarminAuthenticationService
{
    private readonly IAuthParameters _authParameters;
    private readonly HttpClient _httpClient;

    private readonly string _ssoUrl;
    private readonly string _embedUrl;
    private readonly string _signInUrl;
    private readonly string _oAuthConsumerUrl = "https://thegarth.s3.amazonaws.com/oauth_consumer.json";

    public GarminAuthenticationService(HttpClient httpClient, IAuthParameters authParameters)
    {
        _authParameters = authParameters;
        _httpClient = httpClient;

        _ssoUrl = $"https://sso.{_authParameters.Domain}/sso";
        _embedUrl = $"{_ssoUrl}/embed";
        _signInUrl = $"{_ssoUrl}/signin";
    }

    public async Task<OAuth2Token> RefreshGarminAuthenticationAsync(CancellationToken cancellationToken)
    {
        _authParameters.Cookies = await RequestCookies(cancellationToken);
        _authParameters.Csrf = await RequestCsrfToken(cancellationToken);

        var ticket = await GetOAuthTicket(cancellationToken);
        var consumerCredentials = await GetConsumerCredentials(cancellationToken);
        var auth1Token = await GetOAuth1Token(ticket, consumerCredentials, cancellationToken);

        try
        {
            return await GetOAuth2TokenAsync(auth1Token, consumerCredentials, cancellationToken);
        }
        catch (Exception e)
        {
            throw new GarminConnectAuthenticationException("Auth appeared successful but failed to get the OAuth2 token.", e)
            {
                Code = Code.OAuth2TokenNotFound
            };
        }
    }

    private async Task<ConsumerCredentials> GetConsumerCredentials(CancellationToken cancellationToken)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, _oAuthConsumerUrl);
        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, GarminConnectSnakeCaseSerializerContext.Default.ConsumerCredentials)
            ?? throw new InvalidOperationException("Failed to deserialize ConsumerCredentials from Garmin response.");
    }

    private async Task<string> RequestCookies(CancellationToken cancellationToken)
    {
        var queryEmbed = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kv in _authParameters.GetQueryParameters())
        {
            queryEmbed.Add(kv.Key, kv.Value);
        }

        queryEmbed.Add("gauthHost", _ssoUrl);

        var requestUriEmbed = $"{_embedUrl}?{queryEmbed}";

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUriEmbed);
        foreach (var kv in _authParameters.GetHeaders())
        {
            httpRequestMessage.Headers.Add(kv.Key, kv.Value);
        }

        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        if (responseMessage.StatusCode != HttpStatusCode.OK)
        {
            throw new GarminConnectAuthenticationException("Failed to fetch cookies from Garmin.")
            {
                Code = Code.CookiesNotFound
            };
        }

        var headerCookies = responseMessage.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
        var sb = new StringBuilder();
        foreach (var cookie in headerCookies)
        {
            sb.Append($"{cookie};");
        }

        var cookies = sb.ToString();

        if (string.IsNullOrWhiteSpace(cookies))
        {
            throw new GarminConnectAuthenticationException("Found cookies but they are null.")
            {
                Code = Code.CookiesNotFound
            };
        }

        return cookies;
    }

    private async Task<string> RequestCsrfToken(CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>(_authParameters.GetQueryParameters())
        {
            { "gauthHost", _embedUrl },
            { "service", _embedUrl },
            { "source", _embedUrl },
            { "redirectAfterAccountLoginUrl", _embedUrl },
            { "redirectAfterAccountCreationUrl", _embedUrl }
        };

        var queryCsrf = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kv in parameters)
        {
            queryCsrf.Add(kv.Key, kv.Value);
        }

        var requestUriSignin = $"{_signInUrl}?{queryCsrf}";
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUriSignin);
        foreach (var kv in _authParameters.GetHeaders())
        {
            httpRequestMessage.Headers.Add(kv.Key, kv.Value);
        }

        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        if (responseMessage.StatusCode != HttpStatusCode.OK)
        {
            throw new GarminConnectAuthenticationException("Failed to fetch csrf token from Garmin.")
            { Code = Code.CsrfTokenNotFound };
        }

        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        var regexCSRF = new Regex("name=\"_csrf\"\\s+value=\"(.+?)\"", RegexOptions.Compiled | RegexOptions.Multiline);

        var match = regexCSRF.Match(content);

        if (!match.Success)
        {
            throw new GarminConnectAuthenticationException("Failed to find regex match for csrf token.")
            { Code = Code.CsrfTokenNotFound };
        }

        var csrf = match.Groups[1].Value;

        if (string.IsNullOrWhiteSpace(csrf))
        {
            throw new GarminConnectAuthenticationException("Found csrfToken but its null.")
            { Code = Code.CsrfTokenNotFound };
        }

        return match.Groups[1].Value;
    }

    private async Task<string> GetOAuthTicket(CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>(_authParameters.GetQueryParameters())
        {
            { "gauthHost", _embedUrl },
            { "service", _embedUrl },
            { "source", _embedUrl },
            { "redirectAfterAccountLoginUrl", _embedUrl },
            { "redirectAfterAccountCreationUrl", _embedUrl }
        };

        var queryCSRF = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kv in parameters)
        {
            queryCSRF.Add(kv.Key, kv.Value);
        }

        var requestUriSignin = $"{_signInUrl}?{queryCSRF}";
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUriSignin);
        foreach (var kv in _authParameters.GetHeaders())
        {
            httpRequestMessage.Headers.Add(kv.Key, kv.Value);
        }

        httpRequestMessage.Headers.Add("referer", _signInUrl);
        httpRequestMessage.Headers.Add("NK", "NT");
        httpRequestMessage.Content = new FormUrlEncodedContent(_authParameters.GetFormParameters());

        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        if (responseMessage.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden)
        {
            content = content switch
            {
                "error code: 1015" => "temporary blocked by Garmin",
                "error code: 1020" => "temporary blocked by CloudFlare",
                _ => content
            };

            throw new GarminConnectAuthenticationException(
                    $"Garmin Authentication Failed. {responseMessage.StatusCode}: {content}")
            { Code = Code.OAuth1TicketNotFound };
        }

        var regexTicket = new Regex(@"embed\?ticket=([^""]+)""", RegexOptions.Compiled | RegexOptions.Multiline);
        var match = regexTicket.Match(content);

        if (!match.Success)
        {
            throw new GarminConnectAuthenticationException("Failed to find regex match for ticket.")
            {
                Code = Code.OAuth1TicketNotFound
            };
        }

        var ticket = match.Groups[1].Value;
        if (string.IsNullOrWhiteSpace(ticket))
        {
            throw new GarminConnectAuthenticationException("Found ticket but its null.")
            {
                Code = Code.OAuth1TicketNotFound
            };
        }

        return ticket;
    }

    private async Task<OAuth1Token> GetOAuth1Token(string ticket, ConsumerCredentials credentials,
        CancellationToken cancellationToken)
    {
        string oauth1Response;
        try
        {
            var oauthClient = OAuthRequest.ForRequestToken(credentials.ConsumerKey, credentials.ConsumerSecret);
            oauthClient.RequestUrl =
                $"https://connectapi.{_authParameters.Domain}/oauth-service/oauth/preauthorized?ticket={ticket}&login-url=https://sso.garmin.com/sso/embed&accepts-mfa-tokens=true";

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, oauthClient.RequestUrl);
            httpRequestMessage.Headers.Add("User-Agent", _authParameters.UserAgent);
            httpRequestMessage.Headers.Add("Authorization", oauthClient.GetAuthorizationHeader());

            var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

            oauth1Response = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception e)
        {
            throw new GarminConnectAuthenticationException("Auth appeared successful but failed to get the OAuth1 token.", e)
            {
                Code = Code.OAuth1TokenNotFound
            };
        }

        if (string.IsNullOrWhiteSpace(oauth1Response))
        {
            throw new GarminConnectAuthenticationException("Auth appeared successful but returned OAuth1 Token response is null.")
            {
                Code = Code.OAuth1TokenNotFound
            };
        }

        var queryParams = HttpUtility.ParseQueryString(oauth1Response);

        var oAuthToken = queryParams.Get("oauth_token");
        var oAuthTokenSecret = queryParams.Get("oauth_token_secret");

        if (string.IsNullOrWhiteSpace(oAuthToken))
        {
            throw new GarminConnectAuthenticationException($"Auth appeared successful but returned OAuth1 token is null. oauth1Response: {oauth1Response}")
            {
                Code = Code.OAuth1TokenNotFound
            };
        }

        if (string.IsNullOrWhiteSpace(oAuthTokenSecret))
        {
            throw new GarminConnectAuthenticationException($"Auth appeared successful but returned OAuth1 token secret is null. oauth1Response: {oauth1Response}")
            {
                Code = Code.OAuth1TokenNotFound
            };
        }

        return new OAuth1Token
        {
            Token = oAuthToken,
            TokenSecret = oAuthTokenSecret
        };
    }

    private async Task<OAuth2Token> GetOAuth2TokenAsync(OAuth1Token oAuth1Token, ConsumerCredentials credentials,
        CancellationToken cancellationToken)
    {
        var oauth2Client = OAuthRequest.ForProtectedResource("POST", credentials.ConsumerKey,
            credentials.ConsumerSecret, oAuth1Token.Token, oAuth1Token.TokenSecret);
        oauth2Client.RequestUrl = $"https://connectapi.{_authParameters.Domain}/oauth-service/oauth/exchange/user/2.0";

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, oauth2Client.RequestUrl);
        httpRequestMessage.Headers.Add("User-Agent", _authParameters.UserAgent);
        httpRequestMessage.Headers.Add("Authorization", oauth2Client.GetAuthorizationHeader());

        httpRequestMessage.Content = new FormUrlEncodedContent([]);
        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, GarminConnectSnakeCaseSerializerContext.Default.OAuth2Token)
            ?? throw new InvalidOperationException("Failed to deserialize OAuth2Token from Garmin response.");
    }
}
