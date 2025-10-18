namespace FitSyncHub.Zwift.HttpClients.Models.Requests.Events;

public record ZwiftEventFeedRequest
{
    public required DateTimeOffset From { get; set; }
    public required DateTimeOffset To { get; set; }
    public int Limit { get; set; } = 50;
    public int? PageLimit { get; set; }
}
