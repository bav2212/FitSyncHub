using System.Text.Json.Serialization;
using FitSyncHub.Functions.HttpClients.Models.Requests;
using FitSyncHub.Functions.HttpClients.Models.Responses;
using FitSyncHub.Functions.HttpClients.Models.Responses.Activities;
using FitSyncHub.Functions.HttpClients.Models.Responses.Athletes;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(ActivityModelResponse))]
[JsonSerializable(typeof(List<SummaryActivityModelResponse>))]
[JsonSerializable(typeof(DetailedAthleteResponse))]
[JsonSerializable(typeof(ExchangeTokenRequest))]
[JsonSerializable(typeof(ExchangeTokenResponse))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(RefreshTokenResponse))]
internal partial class StravaRestApiSerializerContext : JsonSerializerContext
{
}
