using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Repositories.Abstractions;
using Microsoft.Azure.Cosmos;

namespace FitSyncHub.Functions.Repositories;

public class UserSessionRepository : CosmosDbRepository<UserSession>
{
    public UserSessionRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase("strava").GetContainer("UserSession"))
    {
    }
}
