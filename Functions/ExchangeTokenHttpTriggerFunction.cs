using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.Extensions;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;

namespace StravaWebhooksAzureFunctions.Functions;

public class ExchangeTokenHttpTriggerFunction
{
    private readonly HashSet<string> _expectedScope = [
        "read",
        "activity:write",
        "activity:read",
        "activity:read_all",
        "profile:write",
        "profile:read_all",
        "read_all"
    ];
    private readonly IStravaOAuthHttpClient _stravaHttpClient;

    public ExchangeTokenHttpTriggerFunction(IStravaOAuthHttpClient stravaHttpClient)
    {
        _stravaHttpClient = stravaHttpClient;
    }

    [Function(nameof(ExchangeTokenHttpTriggerFunction))]
    public async Task<ExchangeTokenMultiResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "exchange_token")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<ExchangeTokenHttpTriggerFunction>();
        logger.LogInformation("Started executing function {Function}", nameof(ExchangeTokenHttpTriggerFunction));

        var code = req.Query["code"];
        var scope = req.Query["scope"];

        if (scope is null || !_expectedScope.SetEquals(scope.Split(',')))
        {
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                HttpResponse = req.CreateBadRequest("Invalid scope")
            };
        }

        if (code is null)
        {
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                HttpResponse = req.CreateBadRequest("Code is required")
            };
        }

        var exchangeTokenResponse = await _stravaHttpClient
            .ExchangeTokenAsync(code, executionContext.CancellationToken);
        logger.LogInformation("Exchanged token");

        var athleteId = exchangeTokenResponse.Athlete.Id;
        if (athleteId != Constants.MyAthleteId)
        {
            logger.LogWarning("Skipping, because this athlete is not supported");
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                HttpResponse = req.CreateBadRequest("Athlete is not supported")
            };
        }

        var persistedGrant = new PersistedGrant
        {
            id = exchangeTokenResponse.Athlete.Id.ToString(),
            TokenType = exchangeTokenResponse.TokenType,
            ExpiresAt = exchangeTokenResponse.ExpiresAt,
            ExpiresIn = exchangeTokenResponse.ExpiresIn,
            RefreshToken = exchangeTokenResponse.RefreshToken,
            AccessToken = exchangeTokenResponse.AccessToken,
            AthleteId = exchangeTokenResponse.Athlete.Id,
            AthleteUserName = exchangeTokenResponse.Athlete.Username,
        };

        // Return a response to both HTTP trigger and Azure Cosmos DB output binding.
        return new ExchangeTokenMultiResponse()
        {
            Document = persistedGrant,
            HttpResponse = req.CreateResponse(HttpStatusCode.OK)
        };
    }
}

public record ExchangeTokenMultiResponse
{
    [CosmosDBOutput(
        databaseName: "strava",
        containerName: "PersistedGrant",
        Connection = "AzureWebJobsStorageConnectionString",
        CreateIfNotExists = true,
        PartitionKey = "/id")]
    public required PersistedGrant? Document { get; init; }
    public required HttpResponseData HttpResponse { get; init; }
}
