namespace FitSyncHub.Common.Models;

public sealed record Coordinate
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
