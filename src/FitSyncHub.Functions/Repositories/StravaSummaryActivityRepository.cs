using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Repositories.Abstractions;
using Microsoft.Azure.Cosmos;

namespace FitSyncHub.Functions.Repositories;

public sealed class StravaSummaryActivityRepository : CosmosDbRepository<StravaSummaryActivityData>
{
    public StravaSummaryActivityRepository(CosmosClient cosmosClient)
        : base(cosmosClient.GetDatabase(Constants.CosmosDb.DatabaseName).GetContainer(Constants.CosmosDb.Containers.StravaSummaryActivity))
    {
    }
}
