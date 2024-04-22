using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;
using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(UpdatableActivityRequest))]
internal partial class StravaBrowserSessionSerializerContext : JsonSerializerContext
{
}