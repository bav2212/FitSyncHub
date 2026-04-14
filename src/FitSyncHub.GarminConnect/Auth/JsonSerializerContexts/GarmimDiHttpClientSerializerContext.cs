using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Auth.Models.Response;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GarminDiRefreshTokenResponse))]
public partial class GarmimDiHttpClientSerializerContext : JsonSerializerContext;
