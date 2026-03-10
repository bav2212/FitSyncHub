using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Repositories.Abstractions;
using Microsoft.Azure.Cosmos;

namespace FitSyncHub.Functions.Repositories;

public sealed class StravaStravaOAuthDataRepository : CosmosDbRepository<StravaOAuthData>
{
    public StravaStravaOAuthDataRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase("fit-sync-hub").GetContainer("StravaOAuthData"))
    {
    }
}
