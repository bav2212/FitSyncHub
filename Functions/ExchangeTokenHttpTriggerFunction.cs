using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using System.Net;

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

        string? code = req.Query["code"];
        string? scope = req.Query["scope"];

        if (scope is null || !_expectedScope.SetEquals(scope.Split(',')))
        {
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                HttpResponse = await CreateBadRequestResponse(req, "Invalid scope")
            };
        }

        if (code is null)
        {
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                HttpResponse = await CreateBadRequestResponse(req, "Code is required")
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
                HttpResponse = await CreateBadRequestResponse(req, "Athlete is not supported")
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

    // maybe move this to a helper class
    private static async Task<HttpResponseData> CreateBadRequestResponse(HttpRequestData req, string responseText)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(responseText);
        return response;
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
