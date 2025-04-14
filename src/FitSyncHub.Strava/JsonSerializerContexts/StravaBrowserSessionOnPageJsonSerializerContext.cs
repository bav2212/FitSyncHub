using System.Text.Json.Serialization;
using FitSyncHub.Strava.Models.BrowserSession;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CorrectElevationOnPageModel))]
internal partial class StravaBrowserSessionOnPageJsonSerializerContext : JsonSerializerContext;
