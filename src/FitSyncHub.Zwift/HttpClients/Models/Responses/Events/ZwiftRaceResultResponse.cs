namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

public sealed record ZwiftRaceResultResponse
{
    public required List<ZwiftRaceResultEntryResponse> Entries { get; init; }
}

public sealed record ZwiftRaceResultEntryResponse
{
    public required ZwiftRaceResultActivityData ActivityData { get; init; }
    public int? BibNumber { get; init; }
    public required ZwiftRaceResultCriticalP CriticalP { get; init; }
    public long? EventId { get; init; }
    public long? EventSubgroupId { get; init; }
    public bool? FlaggedCheating { get; init; }
    public bool? FlaggedSandbagging { get; init; }
    public bool? LateJoin { get; init; }
    public required ZwiftRaceResultProfileData ProfileData { get; init; }
    public required long ProfileId { get; init; }
    public bool? Qualified { get; init; }
    public int? Rank { get; init; }
    public long? RankingValue { get; init; }
    public long? RankingValueWinnerDifference { get; init; }
    public ZwiftRaceResultScoreHistory? ScoreHistory { get; init; }
    public required ZwiftRaceResultSensorData SensorData { get; init; }
}

public sealed record ZwiftRaceResultActivityData
{
    public string? ActivityId { get; init; }
    public int? Calories { get; init; }
    public required long DurationInMilliseconds { get; init; }
    public int? ElevationInMeters { get; init; }
    public string? EndDate { get; init; } // kept as string for safety
    public long? EndWorldTime { get; init; }
    public int? MapId { get; init; }
    public long? SegmentDistanceInCentimeters { get; init; }
    public int? SegmentDistanceInMeters { get; init; }
    public string? Sport { get; init; }
    public long? TimePenaltyMs { get; init; }
    public int? WorldId { get; init; }
}

public sealed record ZwiftRaceResultCriticalP
{
    public int? CriticalP15Seconds { get; init; }
    public int? CriticalP1Minute { get; init; }
    public int? CriticalP20Minutes { get; init; }
    public int? CriticalP5Minutes { get; init; }
}

public sealed record ZwiftRaceResultProfileData
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Gender { get; init; }
    public int? HeightInCentimeters { get; init; }
    public string? ImageSrc { get; init; }
    public string? PlayerType { get; init; }
    public int? WeightInGrams { get; init; }
}

public sealed record ZwiftRaceResultScoreHistory
{
    public double? NewScore { get; init; }
    public double? PreviousScore { get; init; }
    public string? ScoreChangeType { get; init; }
}

public sealed record ZwiftRaceResultSensorData
{
    public required int AvgWatts { get; init; }
    public ZwiftRaceResultHeartRateData? HeartRateData { get; init; }
    public bool? PairedSteeringDevice { get; init; }
    public string? PowerType { get; init; }
    public double? TrainerDifficulty { get; init; }
}

public sealed record ZwiftRaceResultHeartRateData
{
    public int? AvgHeartRate { get; init; }
    public bool? HeartRateMonitor { get; init; }
}
