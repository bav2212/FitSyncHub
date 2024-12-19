using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace ZwiftToIntervalsICUConverter.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(IReadOnlyCollection<CreateWorkoutRequestModel>))]
[JsonSerializable(typeof(CreateWorkoutRequestModel))]
[JsonSerializable(typeof(IReadOnlyCollection<AthleteFolderPlanWorkoutsResponse>))]
internal partial class IntervalsIcuSourceGenerationContext : JsonSerializerContext
{
}
