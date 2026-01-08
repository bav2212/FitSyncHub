using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Profiles;
using FitSyncHub.Zwift.JsonSerializerContexts.Converters;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(DateTimeWithoutColonOffsetJsonConverter)]
)]
[JsonSerializable(typeof(ZwiftPlayerProfileResponse))]
internal sealed partial class ZwiftHttpClientProfilesGenerationContext : JsonSerializerContext;
