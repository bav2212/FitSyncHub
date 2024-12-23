namespace FitSyncHub.Zwift.Scrapers;

public record ZwiftInsiderScraperLeadInAndElevationResponse
{
    public required double Length { get; init; }
    public required double Elevation { get; init; }
}

