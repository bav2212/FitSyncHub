using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Auth;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(OAuth2Token))]
[JsonSerializable(typeof(ConsumerCredentials))]
internal partial class GarminConnectOAuthSerializerContext : JsonSerializerContext;
