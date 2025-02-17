using System.Net;
using System.Text.Json;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Strava.HttpClients;


public class StravaCookieAuthHttpClientCached : IStravaCookieAuthHttpClient
{
    private readonly IStravaCookieAuthHttpClient _cookieAuthService;
    private readonly IStravaAuthCookieStorageManager _stravaAuthCookieStorageManager;

    public StravaCookieAuthHttpClientCached(
        IStravaCookieAuthHttpClient cookieAuthService,
        IStravaAuthCookieStorageManager stravaAuthCookieStorageManager)
    {
        _cookieAuthService = cookieAuthService;
        _stravaAuthCookieStorageManager = stravaAuthCookieStorageManager;
    }

    public async Task<CookieLoginResponse> Login(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var (cookies, authenticityToken) = await GetStoredCookies(username, cancellationToken);
        if (cookies is { } && authenticityToken is { } && await CheckCookiesCorrect(cookies, cancellationToken))
        {
            return new CookieLoginResponse
            {
                Success = true,
                Cookies = cookies,
                AuthenticityToken = authenticityToken
            };
        }

        if (cookies is null || authenticityToken is null)
        {
            return new CookieLoginResponse
            {
                Success = false,
                Cookies = null,
                AuthenticityToken = null
            };
        }


        var response = await _cookieAuthService.Login(username, password, cancellationToken);
        if (response.Success)
        {
            var cookiesCollection = cookies.GetAllCookies();
            var cookiesCollectionJson = JsonSerializer
                .Serialize(cookiesCollection, CookieCollectionJsonSerializerContext.Default.CookieCollection);

            await _stravaAuthCookieStorageManager.StoreCookies(username, cookiesCollectionJson, authenticityToken, cancellationToken);
        }
        else
        {
            await _stravaAuthCookieStorageManager.DeleteCookies(username, cancellationToken);
        }

        return response;
    }

    public Task<bool> CheckCookiesCorrect(
        CookieContainer cookies,
        CancellationToken cancellationToken)
    {
        return _cookieAuthService.CheckCookiesCorrect(cookies, cancellationToken);
    }

    private async Task<(CookieContainer? cookies, string? authenticityToken)> GetStoredCookies(
        string username,
        CancellationToken cancellationToken)
    {
        var tuple = await _stravaAuthCookieStorageManager.ReadCookies(username, cancellationToken);

        if (tuple is null)
        {
            return (default, default);
        }

        var cookiesCollection = JsonSerializer.Deserialize(tuple.SerializedCookies,
            CookieCollectionJsonSerializerContext.Default.CookieCollection)!;
        var cookies = new CookieContainer();
        cookies.Add(cookiesCollection);

        var authenticityToken = tuple.AuthenticityToken;

        return (cookies, authenticityToken);
    }
}
