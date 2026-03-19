using System.Text.Json.Serialization;
using FitSyncHub.Strava.HttpClients.Models.Requests;
using FitSyncHub.Strava.HttpClients.Models.Responses;
using FitSyncHub.Strava.HttpClients.Models.Responses.Activities;
using FitSyncHub.Strava.HttpClients.Models.Responses.Athletes;

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
