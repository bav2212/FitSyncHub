using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GarminSetUserWeightRequest))]
[JsonSerializable(typeof(GarminWeightResponse))]
internal partial class GarminConnectWeightSerializerContext : JsonSerializerContext;
