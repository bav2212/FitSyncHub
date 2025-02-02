using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace ZwiftToIntervalsICUConverter.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(IReadOnlyCollection<WorkoutCreateRequest>))]
[JsonSerializable(typeof(WorkoutCreateRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<AthleteFolderPlanWorkoutsResponse>))]
[JsonSerializable(typeof(IReadOnlyCollection<ActivityResponse>))]
[JsonSerializable(typeof(ActivityCreateResponse))]
[JsonSerializable(typeof(ActivityUpdateRequest))]
[JsonSerializable(typeof(EventResponse))]
internal partial class IntervalsIcuSourceGenerationContext : JsonSerializerContext
{
}
