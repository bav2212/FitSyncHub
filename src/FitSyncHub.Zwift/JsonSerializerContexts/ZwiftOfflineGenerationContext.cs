using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftOffline;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ZwiftDataGameInfoResponse))]
internal partial class ZwiftOfflineGenerationContext : JsonSerializerContext;
