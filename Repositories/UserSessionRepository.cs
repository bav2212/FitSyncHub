using Microsoft.Azure.Cosmos;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.Repositories.Abstractions;

namespace StravaWebhooksAzureFunctions.Repositories;

public class UserSessionRepository : CosmosDbRepository<UserSession>
{
    public UserSessionRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase("strava").GetContainer("UserSession"))
    {
    }
}
