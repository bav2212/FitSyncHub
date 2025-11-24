using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Functions.Strava;

public sealed class StravaExchangeTokenHttpTriggerFunction
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
    private readonly IStravaOAuthHttpClient _stravaAuthHttpClient;
    private readonly long _athleteId;
    private readonly ILogger<StravaExchangeTokenHttpTriggerFunction> _logger;

    public StravaExchangeTokenHttpTriggerFunction(
        IStravaOAuthHttpClient stravaAuthHttpClient,
        IOptions<StravaOptions> options,
        ILogger<StravaExchangeTokenHttpTriggerFunction> logger)
    {
        _stravaAuthHttpClient = stravaAuthHttpClient;
        _athleteId = options.Value.AthleteId;
        _logger = logger;
    }

    [Function(nameof(StravaExchangeTokenHttpTriggerFunction))]
    public async Task<ExchangeTokenMultiResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "strava/exchange_token")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started executing function {Function}", nameof(StravaExchangeTokenHttpTriggerFunction));

        string? code = req.Query["code"];
        string? scope = req.Query["scope"];

        if (scope == null || !_expectedScope.SetEquals(scope.Split(',')))
        {
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                Result = new BadRequestObjectResult("Invalid scope")
            };
        }

        if (code is null)
        {
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                Result = new BadRequestObjectResult("Code is required")
            };
        }

        var exchangeTokenResponse = await _stravaAuthHttpClient.ExchangeTokenAsync(code, cancellationToken);
        _logger.LogInformation("Exchanged token");

        var athleteId = exchangeTokenResponse.Athlete.Id;
        if (athleteId != _athleteId)
        {
            _logger.LogWarning("Skipping, because this athlete is not supported");
            return new ExchangeTokenMultiResponse()
            {
                Document = default,
                Result = new BadRequestObjectResult("Athlete is not supported")
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
            Result = new OkResult()
        };
    }
}

public sealed record ExchangeTokenMultiResponse
{
    [CosmosDBOutput(
        databaseName: "fit-sync-hub",
        containerName: "PersistedGrant",
        Connection = "AzureWebJobsStorageConnectionString",
        CreateIfNotExists = true,
        PartitionKey = "/id")]
    public required PersistedGrant? Document { get; init; }
    [HttpResult]
    public required IActionResult Result { get; set; }
}
