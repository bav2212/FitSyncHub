using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.Options;

namespace StravaWebhooksAzureFunctions.Functions;

public class WeightHttpTriggerFunction
{
    private readonly BodyMeasurementsOptions _options;
    private readonly IStravaRestHttpClient _stravaRestHttpClient;

    public WeightHttpTriggerFunction(
        IStravaRestHttpClient stravaRestHttpClient,
        IOptions<BodyMeasurementsOptions> options)
    {
        _options = options.Value;
        _stravaRestHttpClient = stravaRestHttpClient;
    }

    [Function(nameof(WeightHttpTriggerFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "weight")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<WeightHttpTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var weight = req.Query["weight"];
        var athleteId = req.Query["athlete_id"];
        var verifyToken = req.Query["verify_token"];

        if (weight is null || athleteId is null || verifyToken is null)
        {
            return BadRequest("wrong request");
        }

        if (verifyToken != _options.VerifyToken)
        {
            return BadRequest("VerifyToken is wrong");
        }

        if (!float.TryParse(weight, out var parsedWeight))
        {
            return BadRequest("Weight has wrong format");
        }

        if (string.IsNullOrWhiteSpace(athleteId) || !long.TryParse(athleteId, out var parsedAthleteId))
        {
            return BadRequest("Athlete id is not valid");
        }

        logger.LogInformation("Weight is {weight} kg", weight);

        try
        {
            await _stravaRestHttpClient.UpdateAthlete(parsedAthleteId, parsedWeight, executionContext.CancellationToken);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(weight);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot update athlete weight");
            return BadRequest("Cannot update athlete weight");
        }

        HttpResponseData BadRequest(string responseText)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.WriteString(responseText);
            return response;
        }
    }
}
