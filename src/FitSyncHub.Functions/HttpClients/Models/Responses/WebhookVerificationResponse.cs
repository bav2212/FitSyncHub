using System.Text.Json.Serialization;

namespace FitSyncHub.Functions.HttpClients.Models.Responses;

public record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public required string HubChallenge { get; init; }
}
