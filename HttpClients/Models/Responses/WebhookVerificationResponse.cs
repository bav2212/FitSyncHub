using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

public record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public string HubChallenge { get; init; }
}
