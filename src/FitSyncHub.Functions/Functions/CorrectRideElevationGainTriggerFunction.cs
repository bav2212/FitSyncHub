using FitSyncHub.Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace FitSyncHub.Functions.Functions;

public class CorrectRideElevationGainTriggerFunction
{
    private readonly CorrectElevationService _correctElevationService;
    private readonly ILogger<CorrectRideElevationGainTriggerFunction> _logger;

    public CorrectRideElevationGainTriggerFunction(
        CorrectElevationService correctElevationService,
        ILogger<CorrectRideElevationGainTriggerFunction> logger)
    {
        _correctElevationService = correctElevationService;
        _logger = logger;
    }

    [Function(nameof(CorrectRideElevationGainTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "correct-elevation")] HttpRequest req,
        [FromBody] CorrectRideElevationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var result = await _correctElevationService.CorrectElevation(
            request.Before,
            request.After,
            cancellationToken);

        return new OkObjectResult(new
        {
            request.Before,
            request.After,
            Count = result
        });
    }
}

public record CorrectRideElevationRequest
{
    public required DateOnly Before { get; init; }
    public required DateOnly After { get; init; }
}
