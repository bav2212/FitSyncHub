using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using System.Net;
using System.Text.Json;

namespace StravaWebhooksAzureFunctions.HttpClients;

public class StravaCookieAuthHttpClientCached : IStravaCookieAuthHttpClient
{
    private readonly IStravaCookieAuthHttpClient _cookieAuthService;
    private readonly Container _userSessionContainer;

    public StravaCookieAuthHttpClientCached(
        IStravaCookieAuthHttpClient cookieAuthService,
        CosmosClient cosmosClient)
    {
        _cookieAuthService = cookieAuthService;
        _userSessionContainer = cosmosClient.GetContainer("strava", "UserSession");
    }

    public async Task<CookieLoginResponse> Login(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var (cookies, authenticityToken) = await GetStoredCookies(username, cancellationToken);
        if (cookies is { } && authenticityToken is { }
            && await CheckCookiesCorrect(cookies, authenticityToken, cancellationToken))
        {
            return new CookieLoginResponse { Success = true, Cookies = cookies, AuthenticityToken = authenticityToken };
        }

        var response = await _cookieAuthService.Login(username, password, cancellationToken);
        if (response.Success)
        {
            await StoreCookies(username, response.Cookies, response.AuthenticityToken, cancellationToken);
        }
        else
        {
            await DeleteCookies(username, cancellationToken);
        }

        return response;
    }

    public Task<bool> CheckCookiesCorrect(
        CookieContainer cookies,
        string authenticityToken,
        CancellationToken cancellationToken)
    {
        return _cookieAuthService.CheckCookiesCorrect(cookies, authenticityToken, cancellationToken);
    }

    private async Task<(CookieContainer? cookies, string? authenticityToken)> GetStoredCookies(
        string username,
        CancellationToken cancellationToken)
    {
        var iterator = _userSessionContainer
            .GetItemLinqQueryable<UserSession>()
            .Where(x => x.id == username)
            .ToFeedIterator();

        var results = await iterator.ReadNextAsync(cancellationToken);
        var userSession = results.SingleOrDefault();
        if (userSession is not null)
        {
            var cookiesCollection = JsonSerializer
                .Deserialize<CookieCollection>(userSession.CookiesCollectionRawData)!;
            var cookies = new CookieContainer();
            cookies.Add(cookiesCollection);

            var authenticityToken = userSession.AuthenticityToken;

            return (cookies, authenticityToken);
        }

        return (default, default);
    }

    private Task<ItemResponse<UserSession>> StoreCookies(
        string username,
        CookieContainer cookies,
        string authenticityToken,
        CancellationToken cancellationToken)
    {
        var cookiesCollection = cookies.GetAllCookies();
        var cookiesCollectionJson = JsonSerializer.Serialize(cookiesCollection);

        var userSession = new UserSession
        {
            id = username,
            AthleteUserName = username,
            CookiesCollectionRawData = cookiesCollectionJson,
            AuthenticityToken = authenticityToken
        };

        return _userSessionContainer
            .CreateItemAsync(userSession, cancellationToken: cancellationToken);
    }

    private async Task DeleteCookies(
        string username,
        CancellationToken cancellationToken)
    {
        var iterator = _userSessionContainer
           .GetItemLinqQueryable<UserSession>()
           .Where(x => x.id == username)
           .ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var results = await iterator.ReadNextAsync(cancellationToken);

            foreach (var item in results)
            {
                await _userSessionContainer.DeleteItemAsync<UserSession>(item.id, PartitionKey.None, cancellationToken: cancellationToken);
            }
        }
    }
}
