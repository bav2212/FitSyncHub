using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record ActivityStream
{
    public required string Type { get; init; }
    public required string Name { get; init; }
    //Integer[], Float[], Boolean[], int[] (fixed_watts, interpolated_watts), short[][] (hrv)
    public required JsonArray Data { get; init; }
    //Float[] only for type latlng holding the longitude data(latitude is in data)
    public required JsonArray? Data2 { get; init; }
    //Each entry in data is an array of points.For better than 1 Hz custom streams.
    [JsonPropertyName("valueType")]
    public required string ValueType { get; init; }

    [JsonPropertyName("valueTypeIsArray")]
    public bool ValueTypeIsArray { get; init; }
    public required List<ActivityStreamAnomaly> Anomalies { get; init; }
    public bool Custom { get; init; }
    [JsonPropertyName("allNull")]
    public bool AllNull { get; init; }
}

public sealed record ActivityStreamAnomaly
{
    public int StartIndex { get; init; }
    public int EndIndex { get; init; }
    public int Value { get; init; }
    [JsonPropertyName("valueEnd")]
    public int ValueEnd { get; init; }
}
