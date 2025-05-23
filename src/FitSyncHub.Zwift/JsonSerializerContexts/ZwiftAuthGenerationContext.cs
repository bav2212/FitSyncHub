using System.Text.Json.Serialization;
using FitSyncHub.Zwift.Auth;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(ZwiftAuthToken))]
internal partial class ZwiftAuthGenerationContext : JsonSerializerContext;
