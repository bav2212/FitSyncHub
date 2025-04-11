using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(GarminConnectWorkoutResponse))]
internal partial class GarminConnectWorkoutSerializerContext : JsonSerializerContext
{
}
