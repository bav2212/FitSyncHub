using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using StravaWebhooksAzureFunctions.Data.Entities.Abstractions;

namespace StravaWebhooksAzureFunctions.Repositories.Abstractions;

public class CosmosDbRepository<T> where T : DataModel
{
    protected readonly Container Container;

    public CosmosDbRepository(Container container)
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
        return Container.DeleteItemAsync<T>(dataModel.id, PartitionKey.None, cancellationToken: cancellationToken);
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
            foreach (var item in response)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
