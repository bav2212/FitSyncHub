namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record ListActivitiesQueryParams
{
    public ListActivitiesQueryParams(DateTime oldest, DateTime newest)
    {
        Oldest = oldest;
        Newest = newest;
    }

    public ListActivitiesQueryParams(DateOnly oldest, DateOnly newest)
        : this(oldest.ToDateTime(TimeOnly.MinValue), newest.ToDateTime(TimeOnly.MaxValue))
    { }

    public DateTime Oldest { get; }
    public DateTime Newest { get; }
    public int Limit { get; init; } = 10;
}
