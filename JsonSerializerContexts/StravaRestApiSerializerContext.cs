using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Athletes;
using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(ActivityModelResponse))]
[JsonSerializable(typeof(DetailedAthleteResponse))]
[JsonSerializable(typeof(ExchangeTokenRequest))]
[JsonSerializable(typeof(ExchangeTokenResponse))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(RefreshTokenResponse))]
internal partial class StravaRestApiSerializerContext : JsonSerializerContext
{
}
