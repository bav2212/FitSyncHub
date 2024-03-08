using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Options;
using System.Net;
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
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "webhook")] HttpRequestData req,
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
            return BadRequest("wrong request");
        }

        // Verifies that the mode and token sent are valid
        if (mode != "subscribe" || verifyToken != _options.WebhookVerifyToken)
        {
            return BadRequest("WebhookVerifyToken is wrong");
        }

        // Responds with the challenge token from the request
        logger.LogInformation("WEBHOOK_VERIFIED");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new WebhookVerificationResponse
        {
            HubChallenge = challenge
        });

        return response;


        HttpResponseData BadRequest(string responseText)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.WriteString(responseText);
            return response;
        }
    }
}

public record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public required string HubChallenge { get; init; }
}