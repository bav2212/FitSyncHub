using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.JsonSerializerContexts.Converters;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(GarminConnectDateTimeConverter), typeof(GarminConnectDateOnlyConverter)]
)]
[JsonSerializable(typeof(GarminActivitySearchResponse[]))]
internal partial class GarminConnectActivityListSerializerContext : JsonSerializerContext;
