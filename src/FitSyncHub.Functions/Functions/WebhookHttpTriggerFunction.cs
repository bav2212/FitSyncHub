using System.Net;
using System.Text.Json.Serialization;
using FitSyncHub.Functions.Extensions;
using FitSyncHub.Functions.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Functions;

public class WebhookHttpTriggerFunction
{
    private readonly StravaOptions _options;

    public WebhookHttpTriggerFunction(IOptions<StravaOptions> options)
    {
        _options = options.Value;
    }

    [Function(nameof(WebhookHttpTriggerFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "webhook")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<WebhookHttpTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var mode = req.Query["hub.mode"];
        var verifyToken = req.Query["hub.verify_token"];
        var challenge = req.Query["hub.challenge"];

        // Checks if a token and mode is in the query string of the request
        if (mode is null || verifyToken is null || challenge is null)
        {
            return req.CreateBadRequest("wrong request");
        }

        // Verifies that the mode and token sent are valid
        if (mode != "subscribe" || verifyToken != _options.WebhookVerifyToken)
        {
            return req.CreateBadRequest("WebhookVerifyToken is wrong");
        }

        // Responds with the challenge token from the request
        logger.LogInformation("WEBHOOK_VERIFIED");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new WebhookVerificationResponse
        {
            HubChallenge = challenge
        }, executionContext.CancellationToken);

        return response;
    }
}

public record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public required string HubChallenge { get; init; }
}
