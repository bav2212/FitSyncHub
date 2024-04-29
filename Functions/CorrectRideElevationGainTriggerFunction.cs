using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Services;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace StravaWebhooksAzureFunctions.Functions;

public class CorrectRideElevationGainTriggerFunction
{
    private readonly CorrectElevationService _correctElevationService;

    public CorrectRideElevationGainTriggerFunction(CorrectElevationService correctElevationService)
    {
        _correctElevationService = correctElevationService;
    }

    [Function(nameof(CorrectRideElevationGainTriggerFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "correct-elevation")] HttpRequestData req,
        [FromBody] CorrectRideElevationRequest request,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<CorrectRideElevationGainTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        await _correctElevationService.CorrectElevation(request.Before, request.After, executionContext.CancellationToken);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            request.Before,
            request.After
        }, executionContext.CancellationToken);

        return response;
    }
}

public record CorrectRideElevationRequest
{
    public required DateOnly Before { get; init; }
    public required DateOnly After { get; init; }
}
