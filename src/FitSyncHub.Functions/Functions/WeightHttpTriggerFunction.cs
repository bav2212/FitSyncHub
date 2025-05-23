﻿using FitSyncHub.Functions.Options;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.Strava.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Functions;

public class WeightHttpTriggerFunction
{
    private readonly BodyMeasurementsOptions _options;
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly ILogger<WeightHttpTriggerFunction> _logger;

    public WeightHttpTriggerFunction(
        IStravaHttpClient stravaHttpClient,
        GarminConnectHttpClient garminConnectHttpClient,
        IOptions<BodyMeasurementsOptions> options,
        ILogger<WeightHttpTriggerFunction> logger)
    {
        _options = options.Value;
        _stravaHttpClient = stravaHttpClient;
        _garminConnectHttpClient = garminConnectHttpClient;
        _logger = logger;
    }

    [Function(nameof(WeightHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weight")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string? weight = req.Query["weight"];
        string? verifyToken = req.Query["verify_token"];

        if (weight is null || verifyToken is null)
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

        if (parsedWeight > 200)
        {
            return new BadRequestObjectResult("Weight should be in kg's");
        }

        _logger.LogInformation("Weight is {weight} kg", weight);

        try
        {
            _logger.LogInformation("Updating athlete weight on strava");
            await _stravaHttpClient.UpdateAthlete(parsedWeight, cancellationToken);
            _logger.LogInformation("Updated athlete weight on strava");

            _logger.LogInformation("Updating athlete weight on garmin connect");
            await _garminConnectHttpClient.SetUserWeight(parsedWeight * 1000, cancellationToken);
            _logger.LogInformation("Updated athlete weight on garmin connect");

            return new OkObjectResult(weight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot update athlete weight");
            return new BadRequestObjectResult("Cannot update athlete weight");
        }
    }
}
