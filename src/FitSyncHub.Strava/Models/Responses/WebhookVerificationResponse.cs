using System.Text.Json.Serialization;

namespace FitSyncHub.Strava.Models.Responses;

public record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public required string HubChallenge { get; init; }
}
