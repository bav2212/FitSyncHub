using System.Text.Json.Serialization;
using FitSyncHub.Strava.Models.BrowserSession;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(UpdatableActivityRequest))]
internal partial class StravaBrowserSessionSerializerContext : JsonSerializerContext;
