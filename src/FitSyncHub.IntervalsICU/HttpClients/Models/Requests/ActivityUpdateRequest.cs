using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record ActivityUpdateRequest
{
    public ActivitySubType? SubType { get; init; }
    public uint? IcuTrainingLoad { get; init; }
    public bool? Trainer { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Type { get; init; }
    public GearUpdateRequest? Gear { get; init; }
    public uint? IcuRpe { get; init; }
    public uint? Feel { get; init; }
    public uint? IcuFtp { get; init; }
    #region Custom fields
    // JsonPropertyName cause custom fields are CamelCase in IntervalsICU API
    [JsonPropertyName("Lactate")]
    public double? Lactate { get; init; }
    [JsonPropertyName("OutsideAvgWeatherTemp")]
    public double? OutsideAvgWeatherTemp { get; init; }
    [JsonPropertyName("OutsideMinWeatherTemp")]
    public double? OutsideMinWeatherTemp { get; init; }
    [JsonPropertyName("OutsideMaxWeatherTemp")]
    public double? OutsideMaxWeatherTemp { get; init; }

    #endregion Custom fields
}

public sealed record GearUpdateRequest
{
    public required string Id { get; init; }
}
