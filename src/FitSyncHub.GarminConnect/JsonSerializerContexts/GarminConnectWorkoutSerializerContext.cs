using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(WorkoutResponse))]
internal partial class GarminConnectWorkoutSerializerContext : JsonSerializerContext;
