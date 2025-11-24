using System.Text.Json.Serialization;

namespace FitSyncHub.Strava.Models.Responses;

public sealed record WebhookVerificationResponse
{
    [JsonPropertyName("hub.challenge")]
    public required string HubChallenge { get; init; }
}
