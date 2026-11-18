using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
)]
[JsonSerializable(typeof(List<ActivityStream>))]
internal sealed partial class IntervalsActivityStreamsSourceGenerationContext : JsonSerializerContext;
