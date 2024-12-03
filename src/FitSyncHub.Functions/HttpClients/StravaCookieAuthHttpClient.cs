using System.Net;
using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.HttpClients.Models.Responses;
using HtmlAgilityPack;

namespace FitSyncHub.Functions.HttpClients;

public class StravaCookieAuthHttpClient : IStravaCookieAuthHttpClient
{
    private const string StravaUrlLogin = "https://www.strava.com/login";
    private const string StravaUrlSession = "https://www.strava.com/session";
    private const string StravaLoggedOutFingerprint = "logged-out";

    public async Task<CookieLoginResponse> Login(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var cookies = new CookieContainer();
        var handler = new HttpClientHandler() { CookieContainer = cookies };
        var client = new HttpClient(handler);

        var getResponse = await client.GetAsync(StravaUrlLogin, cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(await getResponse.Content.ReadAsStringAsync(cancellationToken));

        var tokenNode = doc.DocumentNode.SelectSingleNode("//input[@name='authenticity_token']");
        var authenticityToken = tokenNode.GetAttributeValue("value", "");

        var content = new FormUrlEncodedContent(
        [
            new("email", username),
            new("password", password),
            new("utf8", "✓"),
            new("authenticity_token", authenticityToken)
        ]);

        var postResponse = await client.PostAsync(StravaUrlSession, content, cancellationToken);
        postResponse.EnsureSuccessStatusCode();

        return new CookieLoginResponse
        {
            Success = await CheckCookiesCorrect(cookies, cancellationToken),
            AuthenticityToken = authenticityToken,
            Cookies = cookies,
        };
    }

    public async Task<bool> CheckCookiesCorrect(
        CookieContainer cookies,
        CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler() { CookieContainer = cookies };
        var client = new HttpClient(handler);

        var dashboardResponse = await client.GetAsync("https://www.strava.com/dashboard", cancellationToken);
        var dashboardHtml = await dashboardResponse.Content.ReadAsStringAsync(cancellationToken);

        return !dashboardHtml.Contains(StravaLoggedOutFingerprint);
    }
}
