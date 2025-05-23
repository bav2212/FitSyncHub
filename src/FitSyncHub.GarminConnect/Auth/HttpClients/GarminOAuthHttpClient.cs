using System.Text.Json;
using System.Web;
using FitSyncHub.GarminConnect.Auth.Exceptions;
using FitSyncHub.GarminConnect.Auth.Models;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using OAuth;

namespace FitSyncHub.GarminConnect.Auth.HttpClients;

internal class GarminOAuthHttpClient
{
    private readonly HttpClient _httpClient;

    public GarminOAuthHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GarminOAuth1Token> GetOAuth1Token(string ticket, GarminConsumerCredentials credentials,
        CancellationToken cancellationToken)
    {
        string oauth1Response;
        try
        {
            const string LoginUrl = "https://sso.garmin.com/sso/embed";

            var oauthClient = OAuthRequest.ForRequestToken(credentials.ConsumerKey, credentials.ConsumerSecret);
            oauthClient.RequestUrl =
                $"{_httpClient.BaseAddress}/preauthorized?ticket={ticket}&login-url={LoginUrl}&accepts-mfa-tokens=true";

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, oauthClient.RequestUrl);
            httpRequestMessage.Headers.Add("Authorization", oauthClient.GetAuthorizationHeader());

            var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

            oauth1Response = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception e)
        {
            throw new GarminConnectAuthenticationException("Auth appeared successful but failed to get the OAuth1 token.", e);
        }

        if (string.IsNullOrWhiteSpace(oauth1Response))
        {
            throw new GarminConnectAuthenticationException("Auth appeared successful but returned OAuth1 Token response is null.");
        }

        var query = HttpUtility.ParseQueryString(oauth1Response);

        var oAuthToken = query.Get("oauth_token");
        var oAuthTokenSecret = query.Get("oauth_token_secret");
        var mfaToken = query.Get("mfa_token");
        var mfaExpirationTimestamp = query.Get("mfa_expiration_timestamp");

        if (string.IsNullOrWhiteSpace(oAuthToken)
            || string.IsNullOrWhiteSpace(oAuthTokenSecret))
        {
            throw new GarminConnectAuthenticationException($"Auth appeared successful but returned OAuth1 token or token secret is null. oauth1Response: {oauth1Response}");
        }

        return new GarminOAuth1Token
        {
            Token = oAuthToken,
            TokenSecret = oAuthTokenSecret,
            MfaToken = mfaToken,
            MfaExpirationTimestamp = mfaExpirationTimestamp,
        };
    }

    public async Task<GarminOAuth2Token> Exchange(
        GarminOAuth1Token oAuth1Token,
        GarminConsumerCredentials credentials,
        CancellationToken cancellationToken)
    {
        var oauth2Client = OAuthRequest.ForProtectedResource("POST",
            credentials.ConsumerKey,
            credentials.ConsumerSecret,
            oAuth1Token.Token,
            oAuth1Token.TokenSecret);
        oauth2Client.RequestUrl = $"{_httpClient.BaseAddress}/exchange/user/2.0";

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, oauth2Client.RequestUrl);
        httpRequestMessage.Headers.Add("Authorization", oauth2Client.GetAuthorizationHeader());

        var formContentDict = new Dictionary<string, string>();
        if (oAuth1Token.MfaToken is not null)
        {
            // python garth uses this, but it doesn't work for me
            //formContentDict.Add("mfa_token", oAuth1Token.MfaToken);
        }

        httpRequestMessage.Content = new FormUrlEncodedContent(formContentDict);
        var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, GarminConnectOAuthSerializerContext.Default.GarminOAuth2Token)
            ?? throw new InvalidOperationException("Failed to deserialize OAuth2Token from Garmin response.");
    }
}
