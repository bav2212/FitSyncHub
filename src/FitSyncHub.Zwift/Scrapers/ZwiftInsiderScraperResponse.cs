namespace FitSyncHub.Zwift.Scrapers;

public record ZwiftInsiderScraperResponse
{
    public required ZwiftInsiderScraperWattPerKgElapsedTimeItemsResponse? WattPerKg { get; init; }
    public required ZwiftInsiderScraperLeadInAndElevationResponse? LeadInAndElevation { get; init; }
}
