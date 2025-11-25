using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftRacingEventResponse>))]
[JsonSerializable(typeof(ZwiftRacingRiderResponse))]
internal sealed partial class ZwiftRacingGenerationContext : JsonSerializerContext;
