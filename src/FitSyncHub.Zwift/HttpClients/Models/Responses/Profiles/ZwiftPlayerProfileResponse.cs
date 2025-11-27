namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Profiles;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;

public sealed record ZwiftPlayerProfileResponse
{
    public int Id { get; init; }
    public string? PublicId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required bool Male { get; init; }
    public string? EventCategory { get; init; }

    public string? ImageSrc { get; init; }
    public string? ImageSrcLarge { get; init; }

    public ZwiftPlayerType PlayerType { get; init; }

    public string? CountryAlpha3 { get; init; }
    public int? CountryCode { get; init; }

    public bool UseMetric { get; init; }
    public bool Riding { get; init; }

    public ZwiftPrivacySettingsResponse? Privacy { get; init; }
    public ZwiftSocialFactsResponse? SocialFacts { get; init; }

    public int? WorldId { get; init; }
    public bool EnrolledZwiftAcademy { get; init; }
    public int PlayerTypeId { get; init; }
    public int? PlayerSubTypeId { get; init; }
    public long? CurrentActivityId { get; init; }
    public bool LikelyInGame { get; init; }
    public string? MarketingConsent { get; init; }
    public string? Address { get; init; }

    public int? Age { get; init; }
    public int? BodyType { get; init; }

    public bool ConnectedToStrava { get; init; }
    public bool ConnectedToTrainingPeaks { get; init; }
    public bool ConnectedToTodaysPlan { get; init; }
    public bool ConnectedToUnderArmour { get; init; }
    public bool ConnectedToWithings { get; init; }
    public bool ConnectedToFitbit { get; init; }
    public bool ConnectedToGarmin { get; init; }
    public bool ConnectedToWahoo { get; init; }
    public bool ConnectedToRuntastic { get; init; }
    public bool ConnectedToZwiftPower { get; init; }

    public bool StravaPremium { get; init; }

    public string? Bt { get; init; }
    public string? Dob { get; init; }
    public string? EmailAddress { get; init; }
    public int? Height { get; init; }

    public string? Location { get; init; }
    public string? PreferredLanguage { get; init; }

    public string? MixpanelDistinctId { get; init; }
    public bool ProfileChanges { get; init; }

    public int? Weight { get; init; }
    public bool B { get; init; }

    public DateTime? CreatedOn { get; init; }
    public string? Source { get; init; }
    public string? Origin { get; init; }

    public string? LaunchedGameClient { get; init; }

    public int? Ftp { get; init; }
    public string? UserAgent { get; init; }

    public int? RunTime1miInSeconds { get; init; }
    public int? RunTime5kmInSeconds { get; init; }
    public int? RunTime10kmInSeconds { get; init; }
    public int? RunTimeHalfMarathonInSeconds { get; init; }
    public int? RunTimeFullMarathonInSeconds { get; init; }

    public string? CyclingOrganization { get; init; }
    public string? LicenseNumber { get; init; }
    public string? BigCommerceId { get; init; }

    public Dictionary<string, string>? PublicAttributes { get; init; }
    public Dictionary<string, string>? PrivateAttributes { get; init; }

    public int AchievementLevel { get; init; }
    public int TotalDistance { get; init; }
    public int TotalDistanceClimbed { get; init; }
    public int TotalTimeInMinutes { get; init; }
    public int TotalInKomJersey { get; init; }
    public int TotalInSprintersJersey { get; init; }
    public int TotalInOrangeJersey { get; init; }
    public int TotalWattHours { get; init; }
    public int TotalExperiencePoints { get; init; }
    public int TargetExperiencePoints { get; init; }
    public int TotalGold { get; init; }

    public int RunAchievementLevel { get; init; }
    public int TotalRunDistance { get; init; }
    public int TotalRunTimeInMinutes { get; init; }
    public int TotalRunExperiencePoints { get; init; }
    public int TargetRunExperiencePoints { get; init; }
    public int TotalRunCalories { get; init; }

    public string? PowerSourceType { get; init; }
    public string? PowerSourceModel { get; init; }
    public string? VirtualBikeModel { get; init; }

    public int NumberOfFolloweesInCommon { get; init; }

    public string? Affiliate { get; init; }
    public string? AvantlinkId { get; init; }
    public string? FundraiserId { get; init; }

    public IReadOnlyCollection<ZwiftProfilePropertyChangesResponse>? ProfilePropertyChanges { get; init; }

    public int StreaksCurrentLength { get; init; }
    public int StreaksMaxLength { get; init; }
    public DateTime? StreaksLastRideTimestamp { get; init; }

    public ZwiftCompetitionMetricsResponse? CompetitionMetrics { get; init; }

    public float TotalWattHoursPerKg { get; init; }
}

public sealed record ZwiftProfilePropertyChangesResponse
{
    public required string PropertyName { get; init; }
    public required int ChangeCount { get; init; }
    public required int MaxChanges { get; init; }
}

public sealed record ZwiftPrivacySettingsResponse
{
    public bool ApprovalRequired { get; init; }
    public bool DisplayWeight { get; init; }
    public bool Minor { get; init; }
    public bool PrivateMessaging { get; init; }
    public bool DefaultFitnessDataPrivacy { get; init; }
    public bool SuppressFollowerNotification { get; init; }
    public bool DisplayAge { get; init; }
    public ZwiftActivityPrivacyType DefaultActivityPrivacy { get; init; }
}

public sealed record ZwiftSocialFactsResponse
{
    public int ProfileId { get; init; }
    public int FollowersCount { get; init; }
    public int FolloweesCount { get; init; }
    public int FolloweesInCommonWithLoggedInPlayer { get; init; }
    public ZwiftFollowStatus FollowerStatusOfLoggedInPlayer { get; init; }
    public ZwiftFollowStatus FolloweeStatusOfLoggedInPlayer { get; init; }
    public bool IsFavoriteOfLoggedInPlayer { get; init; }
}

public sealed record ZwiftCompetitionMetricsResponse
{
    public double? RacingScore { get; init; }
    public string? Category { get; init; }
    public string? CategoryWomen { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ZwiftPlayerType>))]
public enum ZwiftPlayerType
{
    Playertype0 = 0,
    Normal = 1,
    ProCyclist = 2,
    ZwiftStaff = 3,
    Ambassador = 4,
    Verified = 5,
    Zed = 6,
    Zac = 7,
    ProTriathlete = 8,
    ProRunner = 9,
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ZwiftActivityPrivacyType>))]
public enum ZwiftActivityPrivacyType
{
    Public,
    Private,
    Friends
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ZwiftFollowStatus>))]
public enum ZwiftFollowStatus
{
    Followstatus0 = 0,
    Unknown = 1,
    RequestsToFollow = 2,
    IsFollowing = 3,
    IsBlocked = 4,
    NoRelationship = 5,
    Self = 6,
    HasBeenDeclined = 7,
}
