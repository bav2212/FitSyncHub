using System.Text.Json.Serialization;

namespace FitSyncHub.Functions.Data.Entities.Abstractions;

public abstract class DataModel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}
