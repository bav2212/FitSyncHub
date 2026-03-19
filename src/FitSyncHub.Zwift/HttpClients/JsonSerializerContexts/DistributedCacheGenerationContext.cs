using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.JsonSerializerContexts;

[JsonSerializable(typeof(List<long>), TypeInfoPropertyName = "FlammeRougeRacingTourRegisteredRiderIds")]
internal sealed partial class DistributedCacheGenerationContext : JsonSerializerContext;
