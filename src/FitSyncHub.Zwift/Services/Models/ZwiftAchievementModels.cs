namespace FitSyncHub.Zwift.Services.Models;

public sealed record ZwiftAchievementStatus
{
    public required string Name { get; init; }
    public required bool IsAchieved { get; init; }
}

public sealed record ZwiftAchievementProgress
{
    public required List<ZwiftRouteAchievementGroup> RouteAchievements { get; init; }
    public required List<ZwiftAchievementStatus> GeneralAchievements { get; init; }
    public required double AchievementLevel { get; init; }
}

public sealed record ZwiftRouteAchievementGroup
{
    public required string Type { get; init; }
    public required Dictionary<ZwiftAchievementStatus, ZwiftRouteAchievementInfo> Routes { get; init; }
}

public sealed record ZwiftRouteAchievementInfo
{
    public required string Name { get; init; }
    public required DateOnly? PublishedOn { get; init; }
    // temp
    public required double? TotalDistanceInMeters { get; init; }
    // temp
    public required double? TotalAscentInMeters { get; init; }
    public required bool PublicEventsOnly { get; init; }
}
