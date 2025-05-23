using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.JsonSerializerContexts.Converters;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(DateTimeWithoutColonOffsetJsonConverter)]
)]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftEventResponse>))]
[JsonSerializable(typeof(IReadOnlyCollection<ZwiftRaceResultResponse>))]
internal partial class ZwiftEventsGenerationContext : JsonSerializerContext;
