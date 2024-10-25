using System.Text.Json.Serialization;
using FitSyncHub.IntervalsICU.HttpClients.Models;

namespace ZwiftToIntervalsICUConverter.HttpClients.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(IReadOnlyCollection<CreateWorkoutRequestModel>))]
[JsonSerializable(typeof(CreateWorkoutRequestModel))]
internal partial class IntervalsIcuSourceGenerationContext : JsonSerializerContext
{
}