using Microsoft.Azure.Cosmos;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.Repositories.Abstractions;

namespace StravaWebhooksAzureFunctions.Repositories;

public class PersistedGrantRepository : CosmosDbRepository<PersistedGrant>
{
    public PersistedGrantRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase("strava").GetContainer("PersistedGrant"))
    {
    }
}
