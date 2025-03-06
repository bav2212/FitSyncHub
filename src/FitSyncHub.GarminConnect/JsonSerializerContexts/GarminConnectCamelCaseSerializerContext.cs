using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [typeof(GarminConnectDateTimeConverter)]
    )]
[JsonSerializable(typeof(GarminActivityUpdateRequest))]
[JsonSerializable(typeof(GarminSetUserWeightRequest))]
[JsonSerializable(typeof(GarminActivityResponse[]))]
internal partial class GarminConnectCamelCaseSerializerContext : JsonSerializerContext
{
}
