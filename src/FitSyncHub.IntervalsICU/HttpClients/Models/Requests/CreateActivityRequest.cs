using FitSyncHub.Common.Abstractions;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record CreateActivityRequest : IFormDataValue
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? ExternalId { get; init; }
    public int? PairedEventId { get; init; }
}
