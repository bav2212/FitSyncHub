namespace FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;

public record ZwiftRacingRiderResponse
{
    public required int RiderId { get; init; }
    public required bool Male { get; init; }
    public required ZwiftRacingPowerData Power { get; init; }
    public required List<ZwiftRacingHistoryEntry> History { get; init; }
}

public record ZwiftRacingPowerData
{
    public List<double>? Wkg5 { get; init; }
    public List<double>? Wkg15 { get; init; }
    public List<double>? Wkg30 { get; init; }
    public List<double>? Wkg60 { get; init; }
    public List<double>? Wkg120 { get; init; }
    public List<double>? Wkg300 { get; init; }
    public List<double>? Wkg1200 { get; init; }
}

public record ZwiftRacingHistoryEntry
{
    public string? Id { get; init; }
    public ZwiftRacingEventData? Event { get; init; }
    public int? RiderId { get; init; }
    public string? Source { get; init; }
    public string? Name { get; init; }
    public string? TeamId { get; init; }
    public string? TeamName { get; init; }
    public string? Country { get; init; }
    public double? Weight { get; init; }
    public int? Height { get; init; }
    public string? Age { get; init; }
    public bool? Male { get; init; }
    public string? Category { get; init; }
    public double? Time { get; init; }
    public double? TimeGun { get; init; }
    public double? Gap { get; init; }
    public int? Position { get; init; }
    public int? PositionInCategory { get; init; }
    public double? Points { get; init; }   // nullable
    public double? AvgSpeed { get; init; }
    public double? WkgAvg { get; init; }
    public double? Wkg5 { get; init; }
    public double? Wkg15 { get; init; }
    public double? Wkg30 { get; init; }
    public double? Wkg60 { get; init; }
    public double? Wkg120 { get; init; }
    public double? Wkg300 { get; init; }
    public double? Wkg1200 { get; init; }
    public int? Np { get; init; }
    public int? Ftp { get; init; }
    public string? ZpCat { get; init; }
    public double? Load { get; init; }
    public List<object>? RatingScenarios { get; init; }
    public double? Distance { get; init; }
    public ZwiftRacingHeartRate? HeartRate { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public double? Rating { get; init; }
    public double? RatingBefore { get; init; }
    public int? PenTotal { get; init; }
    public int? PenTimeCut { get; init; }
}

public record ZwiftRacingEventData
{
    public string? Id { get; init; }
    public long? Time { get; init; }
    public string? Title { get; init; }
    public string? Type { get; init; }
    public string? SubType { get; init; }
    public double? Distance { get; init; }
    public double? Elevation { get; init; }
    public ZwiftRacingRouteData? Route { get; init; }
}

public record ZwiftRacingRouteData
{
    public string? RouteId { get; init; }
    public string? World { get; init; }
    public string? Name { get; init; }
    public string? Profile { get; init; }
}

public record ZwiftRacingHeartRate
{
    public int? Avg { get; init; }
    public int? Max { get; init; }
}
