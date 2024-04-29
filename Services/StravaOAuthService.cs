using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using StravaWebhooksAzureFunctions.Services.Interfaces;

namespace StravaWebhooksAzureFunctions.Services;

public class StravaOAuthService : IStravaOAuthService
{
    private readonly IStravaOAuthHttpClient _stravaOAuthHttpClient;
    private readonly Container _persistedGrantContainer;

    public StravaOAuthService(
        IStravaOAuthHttpClient stravaOAuthHttpClient,
        CosmosClient cosmosClient)
    {
        _stravaOAuthHttpClient = stravaOAuthHttpClient;
        _persistedGrantContainer = cosmosClient.GetDatabase("strava").GetContainer("PersistedGrant");
    }

    public async Task<TokenResponseModel> RequestToken(long athleteId, CancellationToken cancellationToken)
    {
        var persistedGrant = await GetPersistedGrant(athleteId, cancellationToken)
            ?? throw new NotImplementedException();

        if (DateTimeOffset.FromUnixTimeSeconds(persistedGrant.ExpiresAt) >= DateTimeOffset.UtcNow)
        {
            return new(persistedGrant.AccessToken);
        }

        return await RefreshToken(persistedGrant, cancellationToken);
    }

    private async Task<TokenResponseModel> RefreshToken(PersistedGrant persistedGrant, CancellationToken cancellationToken)
    {
        var refreshTokenResponse = await _stravaOAuthHttpClient
            .RefreshTokenAsync(persistedGrant.RefreshToken, cancellationToken);

        persistedGrant.AccessToken = refreshTokenResponse.AccessToken;
        persistedGrant.RefreshToken = refreshTokenResponse.RefreshToken;
        persistedGrant.ExpiresIn = refreshTokenResponse.ExpiresIn;
        persistedGrant.ExpiresAt = refreshTokenResponse.ExpiresAt;
        persistedGrant.TokenType = refreshTokenResponse.TokenType;

        await _persistedGrantContainer.UpsertItemAsync(persistedGrant, cancellationToken: cancellationToken);

        return new(persistedGrant.AccessToken);
    }

    private async Task<PersistedGrant?> GetPersistedGrant(long athleteId, CancellationToken cancellationToken)
    {
        var matches = _persistedGrantContainer.GetItemLinqQueryable<PersistedGrant>()
                .Where(x => x.AthleteId == athleteId);

        // Convert to feed iterator
        using var linqFeed = matches.ToFeedIterator();

        // Iterate query result pages
        while (linqFeed.HasMoreResults)
        {
            var response = await linqFeed.ReadNextAsync(cancellationToken);

            // Iterate query results
            foreach (var item in response)
            {
                return item;
            }
        }

        return default;
    }
}
