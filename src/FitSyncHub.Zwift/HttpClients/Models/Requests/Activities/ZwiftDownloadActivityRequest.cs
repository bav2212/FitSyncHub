namespace FitSyncHub.Zwift.HttpClients.Models.Requests.Activities;

public sealed record ZwiftDownloadActivityRequest
{
    public required string FitFileBucket { get; init; }
    public required string FitFileKey { get; init; }
}
