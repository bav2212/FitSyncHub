using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record ActivityMessageResponse
{
    public required int Id { get; init; }
    public required string AthleteId { get; init; }
    public required string Name { get; init; }
    public required DateTime Created { get; init; }
    public required ActivityMessageType Type { get; init; }
    public required string Content { get; init; }
    public required string? ActivityId { get; init; }
    public required int? StartIndex { get; init; }
    public required int? EndIndex { get; init; }
    public required string? Answer { get; init; }
    public required object? Activity { get; init; }
    public required string? AttachmentUrl { get; init; }
    public required string? AttachmentMimeType { get; init; }
    public required DateTime? Deleted { get; init; }
    public required string? DeletedById { get; init; }
    public required int? JoinGroupId { get; init; }
    public required int? AcceptCoachingGroupId { get; init; }
    public required bool? Seen { get; init; }
}
