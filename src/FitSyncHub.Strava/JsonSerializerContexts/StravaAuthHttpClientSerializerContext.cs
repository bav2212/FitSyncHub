using System.Text.Json.Serialization;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(ExchangeTokenRequest))]
[JsonSerializable(typeof(ExchangeTokenResponse))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(RefreshTokenResponse))]
internal partial class StravaAuthHttpClientSerializerContext : JsonSerializerContext;
