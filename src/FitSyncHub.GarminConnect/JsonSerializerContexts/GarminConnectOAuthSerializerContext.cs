using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GarminOAuth1Token))]
[JsonSerializable(typeof(GarminOAuth2Token))]
[JsonSerializable(typeof(GarminAuthenticationResult))]
[JsonSerializable(typeof(GarminConsumerCredentials))]
[JsonSerializable(typeof(GarminNeedsMfaClientState))]
public partial class GarminConnectOAuthSerializerContext : JsonSerializerContext;
