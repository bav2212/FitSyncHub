using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Strava.Abstractions;

namespace FitSyncHub.Functions.Managers;

public class StravaAuthCookieStorageManager : IStravaAuthCookieStorageManager
{
    private readonly UserSessionRepository _userSessionRepository;

    public StravaAuthCookieStorageManager(
        UserSessionRepository userSessionRepository)
    {
        _userSessionRepository = userSessionRepository;
    }

    public async Task<SerializedCookieTuple?> ReadCookies(string username, CancellationToken cancellationToken)
    {
        var results = await _userSessionRepository.ReadItems(x => x.id == username, cancellationToken);
        var userSession = results.SingleOrDefault();
        if (userSession is null)
        {
            return default;
        }

        return new SerializedCookieTuple
        {
            AuthenticityToken = userSession.AuthenticityToken,
            SerializedCookies = userSession.CookiesCollectionRawData
        };
    }

    public Task StoreCookies(string username, string serializedCookies, string authenticityToken, CancellationToken cancellationToken)
    {
        var userSession = new UserSession
        {
            id = username,
            AthleteUserName = username,
            CookiesCollectionRawData = serializedCookies,
            AuthenticityToken = authenticityToken
        };

        return _userSessionRepository.UpsertItemAsync(userSession, cancellationToken: cancellationToken);
    }

    public async Task DeleteCookies(
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
