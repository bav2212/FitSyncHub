using FitSyncHub.Functions.Mappers;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models;

namespace FitSyncHub.Functions;

public class CosmosDbStravaOAuthTokenStore : IStravaOAuthTokenStore
{
    private readonly StravaStravaOAuthDataRepository _stravaStravaOAuthDataRepository;
    private readonly StravaStravaOAuthMapper _mapper;

    public CosmosDbStravaOAuthTokenStore(StravaStravaOAuthDataRepository stravaStravaOAuthDataRepository)
    {
        _stravaStravaOAuthDataRepository = stravaStravaOAuthDataRepository;
        _mapper = new StravaStravaOAuthMapper();
    }

    public async Task<StravaOAuthTokenModel> Create(StravaOAuthTokenModel stravaOAuthToken, CancellationToken cancellationToken)
    {
        var dataModel = _mapper.StravaOAuthTokenToDataModel(stravaOAuthToken);

        var createResult = await _stravaStravaOAuthDataRepository.CreateItemAsync(dataModel, cancellationToken);
        return _mapper.DataModelToStravaOAuthToken(createResult.Resource);
    }

    public async Task<StravaOAuthTokenModel> Get(long athleteId, CancellationToken cancellationToken)
    {
        var id = StravaStravaOAuthMapper.MapId(athleteId);
        var dataModel = await _stravaStravaOAuthDataRepository.Read(id, cancellationToken)
            ?? throw new NotImplementedException();

        return _mapper.DataModelToStravaOAuthToken(dataModel);
    }

    public async Task<StravaOAuthTokenModel> Update(StravaOAuthTokenModel stravaOAuthToken, CancellationToken cancellationToken)
    {
        var dataModel = _mapper.StravaOAuthTokenToDataModel(stravaOAuthToken);

        var upsertResult = await _stravaStravaOAuthDataRepository.UpsertItemAsync(dataModel, cancellationToken);

        return _mapper.DataModelToStravaOAuthToken(upsertResult.Resource);
    }
}
