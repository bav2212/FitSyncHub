using System.Text.Json.Serialization;
using FitSyncHub.Functions.HttpClients.Models.BrowserSession;

namespace StravaWebhooksAzureFunctions;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(UpdatableActivityRequest))]
internal partial class StravaBrowserSessionSerializerContext : JsonSerializerContext
{
}
