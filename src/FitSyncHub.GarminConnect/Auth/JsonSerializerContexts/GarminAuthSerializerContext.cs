using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(GarminDiTokenModel))]
[JsonSerializable(typeof(GarminNeedsMfaClientState))]
public partial class GarminAuthSerializerContext : JsonSerializerContext;
