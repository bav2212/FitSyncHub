using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Data.Entities;
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
    private readonly CosmosClient _cosmosClient;

    public ExchangeTokenHttpTriggerFunction(
        IStravaOAuthHttpClient stravaHttpClient,
        CosmosClient cosmosClient)
    {
        _stravaHttpClient = stravaHttpClient;
        _cosmosClient = cosmosClient;
    }

    [Function(nameof(ExchangeTokenHttpTriggerFunction))]
    public async Task<ActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "exchange_token")] HttpRequest req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<ExchangeTokenHttpTriggerFunction>();
        logger.LogInformation("Started executing function {Function}", nameof(ExchangeTokenHttpTriggerFunction));

        string? code = req.Query["code"];
        string? scope = req.Query["scope"];

        if (scope is null || !_expectedScope.SetEquals(scope.Split(',')))
        {
            return new BadRequestObjectResult("Invalid scope");
        }

        if (code is null)
        {
            return new BadRequestObjectResult("Code is required");
        }

        var exchangeTokenResponse = await _stravaHttpClient
            .ExchangeTokenAsync(code, executionContext.CancellationToken);
        logger.LogInformation("Exchanged token");

        var athleteId = exchangeTokenResponse.Athlete.Id;
        if (athleteId != Constants.MyAthleteId)
        {
            logger.LogWarning("Skipping, because this athlete is not supported");
            return new BadRequestObjectResult("Athlete is not supported");
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

        var persistedGrantContainer = _cosmosClient.GetContainer("strava", nameof(PersistedGrant));
        await persistedGrantContainer.UpsertItemAsync(persistedGrant);

        return new OkResult();
    }
}