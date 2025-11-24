namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record AddMessageRequest
{
    public required string Content { get; init; }
}
