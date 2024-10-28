using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Repositories.Abstractions;
using Microsoft.Azure.Cosmos;

namespace FitSyncHub.Functions.Repositories;


public class SummaryActivityRepository : CosmosDbRepository<SummaryActivityData>
{
    public SummaryActivityRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase("fit-sync-hub").GetContainer("SummaryActivity"))
    {
    }
}
