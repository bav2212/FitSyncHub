using System.Text.Json.Serialization;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Models.Responses.Activities;
using FitSyncHub.Strava.Models.Responses.Athletes;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ActivityModelResponse))]
[JsonSerializable(typeof(List<SummaryActivityModelResponse>))]
[JsonSerializable(typeof(DetailedAthleteResponse))]
[JsonSerializable(typeof(StartUploadActivityRequest))]
//WhenWritingNull need for this one
[JsonSerializable(typeof(UpdatableActivityRequest))]
[JsonSerializable(typeof(UploadActivityResponse))]
internal partial class StravaHttpClientSerializerContext : JsonSerializerContext;
