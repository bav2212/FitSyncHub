using System.Text.Json.Serialization;
using FitSyncHub.Common.Abstractions;

namespace FitSyncHub.Strava.Models.Requests;

public record StartUploadActivityRequest : IFormDataValue
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? Commute { get; init; }
    public bool? Trainer { get; init; }
    public required UploadActivityDataType DataType { get; init; }
    public string? ExternalId { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<UploadActivityDataType>))]
public enum UploadActivityDataType
{
    [JsonStringEnumMemberName("fit")]
    Fit,
    [JsonStringEnumMemberName("fit.gz")]
    FitGz,
    [JsonStringEnumMemberName("tcx")]
    Tcx,
    [JsonStringEnumMemberName("tcx.gz")]
    TcxGz,
    [JsonStringEnumMemberName("gpx")]
    Gpx,
    [JsonStringEnumMemberName("gpx.gz")]
    GpxGz,
}
