using System.Text.Json.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Mochi;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(MochiCatalogResponse))]
internal sealed partial class MochiCatalogGenerationContext : JsonSerializerContext;
