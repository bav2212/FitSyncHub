using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Repositories.Abstractions;
using Microsoft.Azure.Cosmos;

namespace FitSyncHub.Functions.Repositories;

public sealed class PersistedGrantRepository : CosmosDbRepository<PersistedGrant>
{
    public PersistedGrantRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase("fit-sync-hub").GetContainer("PersistedGrant"))
    {
    }
}
