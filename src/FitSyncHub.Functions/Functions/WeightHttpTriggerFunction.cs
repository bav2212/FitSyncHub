using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Functions;

public class WeightHttpTriggerFunction
{
    private readonly BodyMeasurementsOptions _options;
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly AthleteContext _athleteContext;
    private readonly ILogger<WeightHttpTriggerFunction> _logger;

    public WeightHttpTriggerFunction(
        IStravaRestHttpClient stravaRestHttpClient,
        AthleteContext athleteContext,
        IOptions<BodyMeasurementsOptions> options,
        ILogger<WeightHttpTriggerFunction> logger)
    {
        _options = options.Value;
        _stravaRestHttpClient = stravaRestHttpClient;
        _athleteContext = athleteContext;
        _logger = logger;
    }

    [Function(nameof(WeightHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weight")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string? weight = req.Query["weight"];
        string? athleteId = req.Query["athlete_id"];
        string? verifyToken = req.Query["verify_token"];

        if (weight is null || athleteId is null || verifyToken is null)
        {
            return new BadRequestObjectResult("wrong request");
        }

        if (verifyToken != _options.VerifyToken)
        {
            return new BadRequestObjectResult("VerifyToken is wrong");
        }

        if (!float.TryParse(weight, out var parsedWeight))
        {
            return new BadRequestObjectResult("Weight has wrong format");
        }

        if (string.IsNullOrWhiteSpace(athleteId) || !long.TryParse(athleteId, out var parsedAthleteId))
        {
            return new BadRequestObjectResult("Athlete id is not valid");
        }

        _logger.LogInformation("Weight is {weight} kg", weight);

        try
        {
            _athleteContext.AthleteId = parsedAthleteId;

            await _stravaRestHttpClient.UpdateAthlete(parsedWeight, cancellationToken);

            return new OkObjectResult(weight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot update athlete weight");
            return new BadRequestObjectResult("Cannot update athlete weight");
        }
    }
}
