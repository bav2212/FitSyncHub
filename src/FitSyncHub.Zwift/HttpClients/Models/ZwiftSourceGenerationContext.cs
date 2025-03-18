using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses;

namespace ZwiftToIntervalsICUConverter.HttpClients.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftEventResponse>))]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftRaceResultResponse>))]
internal partial class ZwiftSourceGenerationContext : JsonSerializerContext
{
}
