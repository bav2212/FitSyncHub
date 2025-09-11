using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public abstract record CreateEventRequestBase
{
    private DateTime _startDateLocal;
    public required DateTime StartDateLocal
    {
        get => _startDateLocal;
        init => _startDateLocal = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }
    public required EventCategory Category { get; init; }
    public required EventType Type { get; init; }
    public EventSubType? SubType { get; init; }
    public bool? Indoor { get; init; }
    public List<string>? Tags { get; init; }
}
