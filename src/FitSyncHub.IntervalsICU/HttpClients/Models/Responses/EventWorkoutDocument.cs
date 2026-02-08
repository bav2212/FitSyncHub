using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record EventWorkoutDocument
{
    // can add many more properties if needed
    public required long Duration { get; init; }
    public float? AverageWatts { get; init; }

    [JsonIgnore]
    public float? WorkCalculated => AverageWatts * Duration;
}

