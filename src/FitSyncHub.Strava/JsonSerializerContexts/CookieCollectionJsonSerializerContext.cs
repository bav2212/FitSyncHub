using System.Net;
using System.Text.Json.Serialization;

namespace FitSyncHub.Functions.JsonSerializerContexts;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(CookieCollection))]
internal partial class CookieCollectionJsonSerializerContext : JsonSerializerContext;
