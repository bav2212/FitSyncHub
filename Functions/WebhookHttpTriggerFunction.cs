using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Options;
using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions.Functions;

public class WebhookHttpTriggerFunction
{
    private readonly StravaOptions _options;

    public WebhookHttpTriggerFunction(IOptions<StravaOptions> options)
    {
        _options = options.Value;
    }

    [Function(nameof(WebhookHttpTriggerFunction))]
    public ActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "webhook")] HttpRequest req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<WebhookHttpTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        string? mode = req.Query["hub.mode"];
        string? verifyToken = req.Query["hub.verify_token"];
        string? challenge = req.Query["hub.challenge"];

        // Checks if a token and mode is in the query string of the request
        if (mode is null || verifyToken is null || challenge is null)
        {
            return new BadRequestObjectResult("wrong request");
        }

        // Verifies that the mode and token sent are valid
        if (mode != "subscribe" || verifyToken != _options.WebhookVerifyToken)
        {
            return new BadRequestObjectResult("WebhookVerifyToken is wrong");
        }

        // Responds with the challenge token from the request
        logger.LogInformation("WEBHOOK_VERIFIED");

        var response = new WebhookVerificationResponse
        {
            HubChallenge = challenge
        };

        return new OkObjectResult(response);
    }
}

public record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public required string HubChallenge { get; init; }
}