using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GarminActivityUpdateRequest))]
[JsonSerializable(typeof(GarminActivityResponse))]
internal partial class GarminConnectActivitySerializerContext : JsonSerializerContext;
