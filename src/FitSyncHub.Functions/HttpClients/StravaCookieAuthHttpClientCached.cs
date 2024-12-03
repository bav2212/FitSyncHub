using System.Net;
using System.Text.Json;
using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.HttpClients.Models.Responses;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Functions.Repositories;
using Microsoft.Azure.Cosmos;

namespace FitSyncHub.Functions.HttpClients;

public class StravaCookieAuthHttpClientCached : IStravaCookieAuthHttpClient
{
    private readonly IStravaCookieAuthHttpClient _cookieAuthService;
    private readonly UserSessionRepository _userSessionRepository;

    public StravaCookieAuthHttpClientCached(
        IStravaCookieAuthHttpClient cookieAuthService,
        UserSessionRepository userSessionRepository)
    {
        _cookieAuthService = cookieAuthService;
        _userSessionRepository = userSessionRepository;
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
            return new CookieLoginResponse
            {
                Success = true,
                Cookies = cookies,
                AuthenticityToken = authenticityToken
            };
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
        var results = await _userSessionRepository.ReadItems(x => x.id == username, cancellationToken);
        var userSession = results.SingleOrDefault();

        if (userSession is null)
        {
            return (default, default);
        }

        var cookiesCollection = JsonSerializer.Deserialize(userSession.CookiesCollectionRawData,
            CookieCollectionJsonSerializerContext.Default.CookieCollection)!;
        var cookies = new CookieContainer();
        cookies.Add(cookiesCollection);

        var authenticityToken = userSession.AuthenticityToken;

        return (cookies, authenticityToken);
    }

    private Task<ItemResponse<UserSession>> StoreCookies(
        string username,
        CookieContainer cookies,
        string authenticityToken,
        CancellationToken cancellationToken)
    {
        var cookiesCollection = cookies.GetAllCookies();
        var cookiesCollectionJson = JsonSerializer.Serialize(cookiesCollection, CookieCollectionJsonSerializerContext.Default.CookieCollection);

        var userSession = new UserSession
        {
            id = username,
            AthleteUserName = username,
            CookiesCollectionRawData = cookiesCollectionJson,
            AuthenticityToken = authenticityToken
        };

        return _userSessionRepository.CreateItemAsync(userSession, cancellationToken: cancellationToken);
    }

    private async Task DeleteCookies(
        string username,
        CancellationToken cancellationToken)
    {
        var results = await _userSessionRepository.ReadItems(x => x.id == username, cancellationToken);

        foreach (var item in results)
        {
            await _userSessionRepository.DeleteItemAsync(item, cancellationToken: cancellationToken);
        }
    }
}
