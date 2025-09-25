using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ZwiftRacingRiderResponse))]
internal partial class ZwiftRacingGenerationContext : JsonSerializerContext;
