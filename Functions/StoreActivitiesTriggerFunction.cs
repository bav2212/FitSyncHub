using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Services;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace StravaWebhooksAzureFunctions.Functions;

public class StoreActivitiesTriggerFunction
{
    private readonly StoreActivitiesService _storeActivitiesService;

    public StoreActivitiesTriggerFunction(StoreActivitiesService storeActivitiesService)
    {
        _storeActivitiesService = storeActivitiesService;
    }

    [Function(nameof(StoreActivitiesTriggerFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "store")] HttpRequestData req,
        [FromBody] StoreStravaActivitiesRequest request,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<StoreActivitiesTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var result = await _storeActivitiesService.StoreActivities(
            Constants.MyAthleteId,
            request.BeforeEpochTime,
            request.AfterEpochTime,
            executionContext.CancellationToken);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            request.BeforeEpochTime,
            request.AfterEpochTime,
            StoredCount = result
        }, executionContext.CancellationToken);

        return response;
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
