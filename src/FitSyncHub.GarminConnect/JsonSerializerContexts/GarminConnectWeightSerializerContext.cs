using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GarminSetUserWeightRequest))]
[JsonSerializable(typeof(GarminWeightResponse))]
public partial class GarminConnectWeightSerializerContext : JsonSerializerContext;
