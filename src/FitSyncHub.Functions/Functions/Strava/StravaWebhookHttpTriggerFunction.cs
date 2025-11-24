using System.Text.Json.Serialization;
using FitSyncHub.Strava.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Functions.Strava;

public sealed class StravaWebhookHttpTriggerFunction
{
    private readonly StravaOptions _options;
    private readonly ILogger<StravaWebhookHttpTriggerFunction> _logger;

    public StravaWebhookHttpTriggerFunction(
        IOptions<StravaOptions> options,
        ILogger<StravaWebhookHttpTriggerFunction> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    [Function(nameof(StravaWebhookHttpTriggerFunction))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "strava/webhook")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

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

        return new OkObjectResult(new WebhookVerificationResponse
        {
            HubChallenge = challenge
        });
    }
}

public sealed record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public required string HubChallenge { get; init; }
}
