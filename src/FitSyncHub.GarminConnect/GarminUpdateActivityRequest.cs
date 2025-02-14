using System.Text.Json.Serialization;

namespace FitSyncHub.GarminConnect;

public class GarminActivityUpdateRequest
{
    [JsonPropertyName("activityId")]
    public required long ActivityId { get; set; }
    [JsonPropertyName("activityName")]
    public required string? ActivityName { get; set; }
    [JsonPropertyName("summaryDTO")]
    public required GarminActivityUpdateSummary SummaryDTO { get; set; }
    [JsonPropertyName("description")]
    public required string? Description { get; set; }
}

public class GarminActivityUpdateSummary
{
    [JsonPropertyName("elevationGain")]
    public required int ElevationGain { get; set; }
    [JsonPropertyName("distance")]
    public required int Distance { get; set; }
}
