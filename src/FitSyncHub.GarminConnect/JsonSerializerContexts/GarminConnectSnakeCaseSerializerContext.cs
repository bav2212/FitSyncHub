using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Auth;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(OAuth2Token))]
[JsonSerializable(typeof(ConsumerCredentials))]
internal partial class GarminConnectSnakeCaseSerializerContext : JsonSerializerContext
{
}
