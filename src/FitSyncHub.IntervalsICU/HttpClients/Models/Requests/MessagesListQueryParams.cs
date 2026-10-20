namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record MessagesListQueryParams
{
    public long? SinceId { get; init; }
    public int Limit { get; init; } = 100;
}
