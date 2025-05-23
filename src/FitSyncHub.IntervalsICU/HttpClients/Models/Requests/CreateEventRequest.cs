using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record CreateEventRequest
{
    public required EventCategory Category { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required DateTime StartDateLocal { get; init; }
    public required string Type { get; init; }
    public required ActivitySubType SubType { get; init; }
    public required bool Indoor { get; init; }
    public required List<string> Tags { get; init; }
}
