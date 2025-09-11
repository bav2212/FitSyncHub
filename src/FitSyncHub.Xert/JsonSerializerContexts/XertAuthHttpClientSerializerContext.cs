using System.Text.Json.Serialization;
using FitSyncHub.Xert.Models.Responses;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(XertTokenResponse))]
internal partial class XertAuthHttpClientSerializerContext : JsonSerializerContext;
