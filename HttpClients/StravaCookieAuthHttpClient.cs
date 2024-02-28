using HtmlAgilityPack;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using System.Net;

namespace StravaWebhooksAzureFunctions.HttpClients;

public class StravaCookieAuthHttpClient : IStravaCookieAuthHttpClient
{
    private const string _stravaUrlLogin = "https://www.strava.com/login";
    private const string _stravaUrlSession = "https://www.strava.com/session";
    private const string _stravaLoggedOutFingerprint = "logged-out";

    public async Task<CookieLoginResponse> Login(string username, string password, CancellationToken cancellationToken)
    {
        var cookies = new CookieContainer();
        var handler = new HttpClientHandler() { CookieContainer = cookies };
        var client = new HttpClient(handler);

        var response = await client.GetAsync(_stravaUrlLogin, cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(await response.Content.ReadAsStringAsync(cancellationToken));

        var tokenNode = doc.DocumentNode.SelectSingleNode("//input[@name='authenticity_token']");
        var authenticityToken = tokenNode.GetAttributeValue("value", "");

        var content = new FormUrlEncodedContent(
        [
            new ("email", username),
            new ("password", password),
            new ("utf8", "✓"),
            new ("authenticity_token", authenticityToken)
        ]);

        response = await client.PostAsync(_stravaUrlSession, content, cancellationToken);
        _ = response;

        return new CookieLoginResponse
        {
            Success = await CheckCookiesCorrect(cookies, authenticityToken, cancellationToken),
            AuthenticityToken = authenticityToken,
            Cookies = cookies,
        };
    }

    public async Task<bool> CheckCookiesCorrect(CookieContainer cookies, string authenticityToken, CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler() { CookieContainer = cookies };
        var client = new HttpClient(handler);

        var dashboardResponse = await client.GetAsync("https://www.strava.com/dashboard", cancellationToken);
        var dashboardHtml = await dashboardResponse.Content.ReadAsStringAsync(cancellationToken);

        return !dashboardHtml.Contains(_stravaLoggedOutFingerprint);
    }
}
