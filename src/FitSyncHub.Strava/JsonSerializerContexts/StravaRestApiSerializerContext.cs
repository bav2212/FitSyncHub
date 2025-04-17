using System.Text.Json.Serialization;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Models.Responses.Activities;
using FitSyncHub.Strava.Models.Responses.Athletes;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(ActivityModelResponse))]
[JsonSerializable(typeof(List<SummaryActivityModelResponse>))]
[JsonSerializable(typeof(DetailedAthleteResponse))]
[JsonSerializable(typeof(ExchangeTokenRequest))]
[JsonSerializable(typeof(ExchangeTokenResponse))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(RefreshTokenResponse))]
[JsonSerializable(typeof(StartUploadActivityRequest))]
[JsonSerializable(typeof(UploadActivityResponse))]
internal partial class StravaRestApiSerializerContext : JsonSerializerContext;
