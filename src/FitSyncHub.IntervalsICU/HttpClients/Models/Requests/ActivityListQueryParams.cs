namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record ActivityListQueryParams
{
    public ActivityListQueryParams(DateTime oldest, DateTime newest)
    {
        Oldest = oldest;
        Newest = newest;
    }

    public ActivityListQueryParams(DateOnly oldest, DateOnly newest)
        : this(oldest.ToDateTime(TimeOnly.MinValue), newest.ToDateTime(TimeOnly.MaxValue))
    { }

    public DateTime Oldest { get; }
    public DateTime Newest { get; }
    public int Limit { get; init; } = 10;
}
