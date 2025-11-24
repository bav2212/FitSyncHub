using System.Linq.Expressions;
using FitSyncHub.Functions.Data.Entities.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace FitSyncHub.Functions.Repositories.Abstractions;

public abstract class CosmosDbRepository<T> where T : DataModel
{
    protected readonly Container Container;

    protected CosmosDbRepository(Container container)
    {
        Container = container;
    }

    public Task<ItemResponse<T>> CreateItemAsync(
       T dataModel,
       CancellationToken cancellationToken)
    {
        return Container.CreateItemAsync(dataModel, cancellationToken: cancellationToken);
    }

    public Task<ItemResponse<T>> UpsertItemAsync(
        T dataModel,
        CancellationToken cancellationToken)
    {
        return Container.UpsertItemAsync(dataModel, cancellationToken: cancellationToken);
    }

    public Task<ItemResponse<T>> DeleteItemAsync(
        T dataModel,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(dataModel.id);

        return Container.DeleteItemAsync<T>(
            dataModel.id, partitionKey, cancellationToken: cancellationToken);
    }

    public async Task<T?> Read(
       Expression<Func<T, bool>> whereExpression,
       CancellationToken cancellationToken)
    {
        var list = await ReadItems(whereExpression, cancellationToken);
        return list.FirstOrDefault();
    }

    public async Task<List<T>> ReadItems(
        Expression<Func<T, bool>> whereExpression,
        CancellationToken cancellationToken)
    {
        var result = new List<T>();

        // Get LINQ IQueryable object
        var queryable = Container.GetItemLinqQueryable<T>();

        // Construct LINQ query
        var matches = queryable.Where(whereExpression);

        var linqFeed = matches.ToFeedIterator();

        // Iterate query result pages
        while (linqFeed.HasMoreResults)
        {
            var response = await linqFeed.ReadNextAsync(cancellationToken);

            // Iterate query results
            result.AddRange(response);
        }

        return result;
    }
}
