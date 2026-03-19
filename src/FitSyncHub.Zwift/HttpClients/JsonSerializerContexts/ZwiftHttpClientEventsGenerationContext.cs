using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.JsonSerializerContexts.Converters;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(DateTimeWithoutColonOffsetJsonConverter)]
)]
[JsonSerializable(typeof(ZwiftEventFeedResponse))]
[JsonSerializable(typeof(ZwiftRaceResultResponse))]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftEventResponse>))]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftEventSubgroupEntrantResponse>))]
internal sealed partial class ZwiftHttpClientEventsGenerationContext : JsonSerializerContext;
