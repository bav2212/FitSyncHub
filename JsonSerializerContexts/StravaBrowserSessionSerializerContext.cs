using System.Text.Json.Serialization;
using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;

namespace StravaWebhooksAzureFunctions;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(UpdatableActivityRequest))]
internal partial class StravaBrowserSessionSerializerContext : JsonSerializerContext
{
}
