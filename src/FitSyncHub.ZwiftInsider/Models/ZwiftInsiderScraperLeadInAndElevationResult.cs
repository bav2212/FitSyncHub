namespace FitSyncHub.ZwiftInsider.Models;

public record ZwiftInsiderScraperLeadInAndElevationResult
{
    public required double Length { get; init; }
    public required double Elevation { get; init; }
}
