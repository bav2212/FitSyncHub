using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

namespace FitSyncHub.IntervalsICU.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    // be careful with this setting, ActivityUpdateRequest need this to be set to WhenWritingNull
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ActivityUpdateRequest))]
internal partial class IntervalsActivityUpdateSourceGenerationContext : JsonSerializerContext;
