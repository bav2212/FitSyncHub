using FitSyncHub.Functions.Mappers;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models;

namespace FitSyncHub.Functions;

public class CosmosDbStravaOAuthTokenStore : IStravaOAuthTokenStore
{
    private readonly PersistedGrantRepository _persistedGrantRepository;
    private readonly StravaPersistedGrantMapper _mapper;

    public CosmosDbStravaOAuthTokenStore(PersistedGrantRepository persistedGrantRepository)
    {
        _persistedGrantRepository = persistedGrantRepository;
        _mapper = new StravaPersistedGrantMapper();
    }

    public Task<StravaOAuthTokenModel> Create(StravaOAuthTokenModel stravaOAuthToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<StravaOAuthTokenModel> Get(long athleteId, CancellationToken cancellationToken)
    {
        var id = StravaPersistedGrantMapper.MapId(athleteId);
        var persistedGrant = await _persistedGrantRepository.Read(id, cancellationToken)
            ?? throw new NotImplementedException();

        return _mapper.PersistedGrantToStravaOAuthToken(persistedGrant);
    }

    public async Task<StravaOAuthTokenModel> Update(StravaOAuthTokenModel stravaOAuthToken, CancellationToken cancellationToken)
    {
        var persistedGrant = _mapper.StravaOAuthTokenToPersistedGrant(stravaOAuthToken);

        var upsertResult = await _persistedGrantRepository.UpsertItemAsync(persistedGrant, cancellationToken);

        return _mapper.PersistedGrantToStravaOAuthToken(upsertResult.Resource);
    }
}
