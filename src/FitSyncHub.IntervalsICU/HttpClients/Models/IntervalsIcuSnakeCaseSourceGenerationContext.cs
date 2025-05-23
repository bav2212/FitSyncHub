using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(IReadOnlyCollection<WorkoutCreateRequest>))]
[JsonSerializable(typeof(WorkoutCreateRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<AthleteFolderPlanWorkoutsResponse>))]
[JsonSerializable(typeof(IReadOnlyCollection<ActivityResponse>))]
[JsonSerializable(typeof(ActivityCreateResponse))]
[JsonSerializable(typeof(CreateActivityRequest))]
[JsonSerializable(typeof(IReadOnlyCollection<EventResponse>))]
[JsonSerializable(typeof(CreateEventRequest))]
internal partial class IntervalsIcuSnakeCaseSourceGenerationContext : JsonSerializerContext;
