namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

public record ZwiftEventSubgroupEntrantResponse
{
    public required long Id { get; init; }
    public required string PublicId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required bool Male { get; init; }
    public required string EventCategory { get; init; }
    public required uint Age { get; init; }
    public required int BodyType { get; init; }
    public required int Height { get; init; }   // in mm
    public required uint Weight { get; init; }   // in grams
    public required uint Ftp { get; init; }
    public required int AchievementLevel { get; init; }
    public required long TotalDistance { get; init; }          // meters
    public required long TotalDistanceClimbed { get; init; }   // meters
    public required int TotalTimeInMinutes { get; init; }
    public required int TotalInKomJersey { get; init; }
    public required int TotalInSprintersJersey { get; init; }
    public required int TotalInOrangeJersey { get; init; }
    public required int TotalWattHours { get; init; }
    public required int TotalExperiencePoints { get; init; }
    public required int TargetExperiencePoints { get; init; }
    public required long TotalGold { get; init; }
    public required int StreaksCurrentLength { get; init; }
    public required double TotalWattHoursPerKg { get; init; }
}
