namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record CreateEventFromFileRequest : CreateEventRequestBase
{
    public required string FileContentsBase64 { get; init; }
    public string? Filename { get; init; }
}
