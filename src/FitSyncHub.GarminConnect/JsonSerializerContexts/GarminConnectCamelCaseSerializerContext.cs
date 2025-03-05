using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GarminActivityUpdateRequest))]
[JsonSerializable(typeof(GarminSetUserWeightRequest))]
[JsonSerializable(typeof(GarminActivityResponse[]))]
internal partial class GarminConnectCamelCaseSerializerContext : JsonSerializerContext
{
}
