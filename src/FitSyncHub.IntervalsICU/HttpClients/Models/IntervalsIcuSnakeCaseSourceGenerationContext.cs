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
[JsonSerializable(typeof(CreateActivityRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<EventResponse>))]
[JsonSerializable(typeof(CreateEventFromDescriptionRequest))]
[JsonSerializable(typeof(CreateEventFromFileRequest))]
[JsonSerializable(typeof(AddMessageRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<ActivityMessageResponse>))]
internal partial class IntervalsIcuSnakeCaseSourceGenerationContext : JsonSerializerContext;
