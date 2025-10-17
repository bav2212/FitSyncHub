namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

public class ZwiftEventFeedResponse
{
    public required List<ZwiftEventFeedItem> Data { get; set; }
    public required string? Cursor { get; set; }
}

public class ZwiftEventFeedItem
{
    public required ZwiftEventResponse Event { get; set; }
}
