using System.Text.Json.Serialization;
using FitSyncHub.Strava.HttpClients.Models.Requests;
using FitSyncHub.Strava.HttpClients.Models.Responses;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(ExchangeTokenRequest))]
[JsonSerializable(typeof(ExchangeTokenResponse))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(RefreshTokenResponse))]
internal partial class StravaAuthHttpClientSerializerContext : JsonSerializerContext;
