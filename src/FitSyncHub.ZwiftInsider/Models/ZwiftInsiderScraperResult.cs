namespace FitSyncHub.ZwiftInsider.Models;

public record ZwiftInsiderScraperResult
{
    public required ZwiftInsiderScraperWattPerKgElapsedTimeItemsResult? WattPerKg { get; init; }
    public required ZwiftInsiderScraperLeadInAndElevationResult? LeadInAndElevation { get; init; }
}
