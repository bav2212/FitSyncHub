using FitSyncHub.IntervalsICU.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.IntervalsIcu;

public class IntervalsICUDeletePlanHttpTriggerFunction
{
    private readonly IntervalsIcuDeletePlanService _intervalsIcuDeletePlanService;
    private readonly ILogger<IntervalsICUDeletePlanHttpTriggerFunction> _logger;

    public IntervalsICUDeletePlanHttpTriggerFunction(
        IntervalsIcuDeletePlanService intervalsIcuDeletePlanService,
        ILogger<IntervalsICUDeletePlanHttpTriggerFunction> logger)
    {
        _intervalsIcuDeletePlanService = intervalsIcuDeletePlanService;
        _logger = logger;
    }

    [Function(nameof(IntervalsICUDeletePlanHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "intervals-icu-delete-plan/{folderId}")]
        HttpRequest req,
        int folderId,
        CancellationToken cancellationToken)
    {
        _ = req;

        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            await _intervalsIcuDeletePlanService.DeleteWorkouts(folderId, cancellationToken);

            _logger.LogInformation("Deleted workouts from folder {FolderId}", folderId);
            return new OkObjectResult($"Deleted workouts from folder {folderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot delete workouts from Intervals.ICU folder");
            return new BadRequestObjectResult("Cannot delete workouts from Intervals.ICU folder");
        }
    }
}
