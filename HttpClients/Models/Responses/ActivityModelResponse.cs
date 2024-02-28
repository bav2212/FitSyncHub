using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

public class ActivityModelResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }
    [JsonPropertyName("distance")]
    public double Distance { get; init; }
    [JsonPropertyName("moving_time")]
    public int MovingTime { get; init; }
    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; init; }
    [JsonPropertyName("type")]
    public string Type { get; init; }
    [JsonPropertyName("sport_type")]
    public string SportType { get; init; }
    [JsonPropertyName("id")]
    public long Id { get; init; }
}
