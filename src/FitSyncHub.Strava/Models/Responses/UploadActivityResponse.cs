namespace FitSyncHub.Strava.Models.Responses;

public sealed record UploadActivityResponse
{
    public required long Id { get; init; }
    public required string IdStr { get; init; }
    public required string ExternalId { get; init; }
    public required string? Error { get; init; }
    public required string Status { get; init; }
    public required long? ActivityId { get; init; }
}
