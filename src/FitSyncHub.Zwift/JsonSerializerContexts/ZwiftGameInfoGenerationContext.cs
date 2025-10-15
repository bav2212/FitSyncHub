using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ZwiftGameInfoResponse))]
internal partial class ZwiftGameInfoGenerationContext : JsonSerializerContext;
