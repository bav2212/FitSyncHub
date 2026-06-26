using System.Text.Json.Serialization;
using FitSyncHub.Weather.HttpClients.JsonSerializerContexts.Converters;
using FitSyncHub.Weather.HttpClients.Models.Responses;

namespace FitSyncHub.Weather.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    Converters = [typeof(GMTDateTimeOffsetConverter)])]
[JsonSerializable(typeof(OpenMeteoResponse))]
internal sealed partial class OpenMeteoArchiveGenerationContext : JsonSerializerContext;
