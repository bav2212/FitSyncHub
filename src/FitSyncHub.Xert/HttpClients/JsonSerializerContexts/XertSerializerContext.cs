using System.Text.Json.Serialization;
using FitSyncHub.Xert.Models;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(TokenModel))]
internal partial class XertSerializerContext : JsonSerializerContext;

