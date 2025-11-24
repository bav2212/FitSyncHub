namespace FitSyncHub.Common.Models;

public sealed record FileModel
{
    public required byte[] Bytes { get; init; }
    public required string Name { get; init; }
}
