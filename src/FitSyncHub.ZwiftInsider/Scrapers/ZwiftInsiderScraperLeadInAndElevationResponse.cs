namespace FitSyncHub.ZwiftInsider.Scrapers;

public record ZwiftInsiderScraperLeadInAndElevationResponse
{
    public required double Length { get; init; }
    public required double Elevation { get; init; }
}
