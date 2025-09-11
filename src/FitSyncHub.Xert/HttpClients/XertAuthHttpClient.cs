using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Xert.Abstractions;
using FitSyncHub.Xert.Models.Responses;
using FitSyncHub.Xert.Options;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Xert.HttpClients;

public class XertAuthHttpClient : IXertAuthHttpClient
{
    private const string Url = "/oauth/token";
    private readonly string _base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes("xert_public:xert_public"));

    private readonly HttpClient _httpClient;
    private readonly XertOptions.XertAuthOptions _xertAuthOptions;

    public XertAuthHttpClient(
        HttpClient httpClient,
        IOptions<XertOptions> xertOptions)
    {
        _httpClient = httpClient;
        _xertAuthOptions = xertOptions.Value.Credentials;
    }

    public async Task<XertTokenResponse> ObtainTokenAsync(CancellationToken cancellationToken)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Url);

        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", _base64EncodedAuthenticationString);

        httpRequestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "username", _xertAuthOptions.Username},
            { "password", _xertAuthOptions.Password},
            { "grant_type", "password" },
        });

        var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(
            XertAuthHttpClientSerializerContext.Default.XertTokenResponse,
            cancellationToken) ?? throw new InvalidDataException();
    }

    public async Task<XertTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Url);

        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", _base64EncodedAuthenticationString);

        httpRequestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "refresh_token", refreshToken},
            { "grant_type", "refresh_token" },
        });

        var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(
            XertAuthHttpClientSerializerContext.Default.XertTokenResponse,
            cancellationToken) ?? throw new InvalidDataException();
    }
}
