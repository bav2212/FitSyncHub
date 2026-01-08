using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Activities;
using FitSyncHub.Zwift.JsonSerializerContexts.Converters;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(DateTimeWithoutColonOffsetJsonConverter)]
)]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftActivityOverview>))]
internal sealed partial class ZwiftHttpClientActivitiesGenerationContext : JsonSerializerContext;
