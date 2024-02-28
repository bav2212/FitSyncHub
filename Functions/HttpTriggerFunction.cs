using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using StravaWebhooksAzureFunctions.Models;

namespace StravaWebhooksAzureFunctions.Functions;

public class HttpTriggerFunction
{
    [Function("HttpExample")]
    public static async Task<MultiResponse> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("HttpExample");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var message = "Welcome to Azure Functions!";

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(message);

        // Return a response to both HTTP trigger and Azure Cosmos DB output binding.
        return new MultiResponse()
        {
            Document = new MyDocument()
            {
                id = Guid.NewGuid().ToString(),
                Boolean = true,
                Number = 1,
                Text = message
            },
            HttpResponse = response
        };
    }
}

public struct MultiResponse
{
    [CosmosDBOutput(
        databaseName: "strava",
        containerName: "UserSession",
        Connection = "AzureWebJobsStorageConnectionString",
        CreateIfNotExists = true)]
    public required MyDocument Document { get; set; }
    public required HttpResponseData HttpResponse { get; set; }
}
