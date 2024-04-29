using System.Text.Json.Serialization;
using StravaWebhooksAzureFunctions.HttpClients.Models.BrowserSession;

namespace StravaWebhooksAzureFunctions;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CorrectElevationOnPageModel))]
internal partial class StravaBrowserSessionOnPageJsonSerializerContext : JsonSerializerContext
{
}
