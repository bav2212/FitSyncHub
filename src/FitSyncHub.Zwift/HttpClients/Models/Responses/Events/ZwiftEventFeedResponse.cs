namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

public sealed class ZwiftEventFeedResponse
{
    public required List<ZwiftEventFeedItem> Data { get; set; }
    public required string? Cursor { get; set; }
}

public sealed class ZwiftEventFeedItem
{
    public required ZwiftEventResponse Event { get; set; }
}
