namespace FitSyncHub.Common.Models;

public record FileModel
{
    public required byte[] Bytes { get; init; }
    public required string Name { get; init; }
}
