using System.Text.Json.Serialization;
using FitSyncHub.Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace FitSyncHub.Functions.Functions;

public class StoreActivitiesTriggerFunction
{
    private readonly StoreSummaryActivitiesService _storeActivitiesService;
    private readonly ILogger<StoreActivitiesTriggerFunction> _logger;

    public StoreActivitiesTriggerFunction(
        StoreSummaryActivitiesService storeActivitiesService,
        ILogger<StoreActivitiesTriggerFunction> logger)
    {
        _storeActivitiesService = storeActivitiesService;
        _logger = logger;
    }

    [Function(nameof(StoreActivitiesTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "store")] HttpRequest req,
        [FromBody] StoreStravaActivitiesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var result = await _storeActivitiesService.StoreSummaryActivities(
            Constants.MyAthleteId,
            request.BeforeEpochTime,
            request.AfterEpochTime,
            cancellationToken);

        return new OkObjectResult(new
        {
            request.BeforeEpochTime,
            request.AfterEpochTime,
            StoredCount = result
        });
    }
}

public record StoreStravaActivitiesRequest
{
    public required DateOnly Before { private get; init; }
    public required DateOnly After { private get; init; }

    [JsonIgnore]
    public long BeforeEpochTime => new DateTimeOffset(Before, TimeOnly.MaxValue, TimeSpan.Zero).ToUnixTimeSeconds();
    [JsonIgnore]
    public long AfterEpochTime => new DateTimeOffset(After, TimeOnly.MinValue, TimeSpan.Zero).ToUnixTimeSeconds();
}
