using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(IReadOnlyCollection<WorkoutCreateRequest>))]
[JsonSerializable(typeof(WorkoutCreateRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<AthleteFolderPlanWorkoutsResponse>))]
[JsonSerializable(typeof(IReadOnlyCollection<ActivityResponse>))]
[JsonSerializable(typeof(ActivityCreateResponse))]
[JsonSerializable(typeof(ActivityCreateRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<EventResponse>))]
[JsonSerializable(typeof(EventCreateFromDescriptionRequest))]
[JsonSerializable(typeof(EventCreateFromFileRequest))]
[JsonSerializable(typeof(MessageAddRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<ActivityMessageResponse>))]
internal sealed partial class IntervalsIcuSnakeCaseSourceGenerationContext : JsonSerializerContext;
