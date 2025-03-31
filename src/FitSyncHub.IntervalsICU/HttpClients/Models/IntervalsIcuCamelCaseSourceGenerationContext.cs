using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace ZwiftToIntervalsICUConverter.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    // be careful with this setting, WellnessRequest need this to be set to WhenWritingNull
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(WellnessRequest))]
[JsonSerializable(typeof(WellnessResponse))]
internal partial class IntervalsIcuCamelCaseSourceGenerationContext : JsonSerializerContext
{
}
