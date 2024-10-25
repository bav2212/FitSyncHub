using System.Text.Json.Serialization;
using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Athletes;

namespace StravaWebhooksAzureFunctions;

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
