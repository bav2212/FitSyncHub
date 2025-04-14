using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.JsonSerializerContexts.Converters;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(GarminConnectDateTimeConverter), typeof(GarminConnectDateOnlyConverter)]
)]
[JsonSerializable(typeof(GarminActivityUpdateRequest))]
[JsonSerializable(typeof(GarminActivityResponse[]))]
[JsonSerializable(typeof(GarminSetUserWeightRequest))]
[JsonSerializable(typeof(GarminWeightResponse))]
internal partial class GarminConnectCamelCaseSerializerContext : JsonSerializerContext;
