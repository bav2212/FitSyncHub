using System.Text.Json.Serialization;
using FitSyncHub.Xert.Models.Responses;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TrainingInfoResponse))]
internal partial class XertHttpClientSerializerContext : JsonSerializerContext;
