namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record EventCreateFromFileRequest : EventCreateRequestBase
{
    public required string FileContentsBase64 { get; init; }
    public string? Filename { get; init; }
}
