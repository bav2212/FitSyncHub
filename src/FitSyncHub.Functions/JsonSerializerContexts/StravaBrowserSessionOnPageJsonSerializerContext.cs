using System.Text.Json.Serialization;
using FitSyncHub.Functions.HttpClients.Models.BrowserSession;

namespace StravaWebhooksAzureFunctions;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CorrectElevationOnPageModel))]
internal partial class StravaBrowserSessionOnPageJsonSerializerContext : JsonSerializerContext
{
}
