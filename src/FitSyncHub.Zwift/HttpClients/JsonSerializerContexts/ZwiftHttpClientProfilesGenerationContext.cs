using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.JsonSerializerContexts.Converters;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Profiles;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(DateTimeWithoutColonOffsetJsonConverter)]
)]
[JsonSerializable(typeof(ZwiftPlayerProfileResponse))]
internal sealed partial class ZwiftHttpClientProfilesGenerationContext : JsonSerializerContext;
