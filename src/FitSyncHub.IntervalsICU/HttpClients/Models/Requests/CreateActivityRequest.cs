namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record CreateActivityRequest
{
    public required byte[] ActivityBytes { get; init; }
    public required string ActivityFileName { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? ExternalId { get; init; }
    public int? PairedEventId { get; init; }
}
