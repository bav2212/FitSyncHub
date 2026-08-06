using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record EventListQueryParams
{
    public EventListQueryParams(DateTime oldest, DateTime newest) : this(DateOnly.FromDateTime(oldest), DateOnly.FromDateTime(newest))
    { }

    public EventListQueryParams(DateOnly oldest, DateOnly newest)
    {
        Oldest = oldest;
        Newest = newest;
    }

    public DateOnly Oldest { get; }
    public DateOnly Newest { get; }
    public EventCategory[]? Category { get; init; }
    public int? Limit { get; init; }
}
