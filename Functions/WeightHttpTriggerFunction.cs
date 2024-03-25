using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Options;
using System.Net;

namespace StravaWebhooksAzureFunctions.Functions;

public class WeightHttpTriggerFunction
{
    private readonly BodyMeasurementsOptions _options;

    public WeightHttpTriggerFunction(IOptions<BodyMeasurementsOptions> options)
    {
        _options = options.Value;
    }

    [Function(nameof(WeightHttpTriggerFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weight")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<WeightHttpTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        string? weight = req.Query["weight"];
        string? verifyToken = req.Query["verify_token"];

        if (verifyToken is null || weight is null)
        {
            return BadRequest("wrong request");
        }

        if (verifyToken != _options.VerifyToken)
        {
            return BadRequest("VerifyToken is wrong");
        }

        if (!double.TryParse(weight, out _))
        {
            return BadRequest("Weight has wrong format");
        }

        logger.LogInformation("Weight is {weight} kg", weight);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(weight);

        return response;


        HttpResponseData BadRequest(string responseText)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.WriteString(responseText);
            return response;
        }
    }
}
