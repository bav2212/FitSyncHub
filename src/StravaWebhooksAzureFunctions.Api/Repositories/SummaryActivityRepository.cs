using Microsoft.Azure.Cosmos;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.Repositories.Abstractions;

namespace StravaWebhooksAzureFunctions.Repositories;


public class SummaryActivityRepository : CosmosDbRepository<SummaryActivityData>
{
    public SummaryActivityRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase("strava").GetContainer("SummaryActivity"))
    {
    }
}
