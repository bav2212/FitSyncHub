using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Models;

namespace StravaWebhooksAzureFunctions.Functions;

public class CosmosDBTriggerFunction
{
    private readonly ILogger _logger;

    public CosmosDBTriggerFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<CosmosDBTriggerFunction>();
    }

    [Function("Function1")]
    public void Run([CosmosDBTrigger(
        databaseName: "strava",
        containerName: "UserSession",
        Connection = "AzureWebJobsStorageConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input)
    {
        if (input != null && input.Count > 0)
        {
            _logger.LogInformation("Documents modified: " + input.Count);
            _logger.LogInformation("First document Id: " + input[0].id);
        }
    }
}
