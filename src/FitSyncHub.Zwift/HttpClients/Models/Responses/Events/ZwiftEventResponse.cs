using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

public sealed record ZwiftEventResponse
{
    public required long Id { get; init; }
    public required int WorldId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string? ShortName { get; init; }
    public required string? ShortDescription { get; init; }
    public required string ImageUrl { get; init; }
    public required int RulesId { get; init; }
    public required int? MapId { get; init; }
    public required long RouteId { get; init; }
    public required string? RouteUrl { get; init; }
    public required long? JerseyHash { get; init; }
    public required long? BikeHash { get; init; }
    public required bool Visible { get; init; }
    public required bool OverrideMapPreferences { get; init; }
    public required DateTime EventStart { get; init; }
    public required int DurationInSeconds { get; init; }
    public required double DistanceInMeters { get; init; }
    public required int? Laps { get; init; }
    public required bool PrivateEvent { get; init; }
    public required bool InvisibleToNonParticipants { get; init; }
    public required int FolloweeEntrantCount { get; init; }
    public required int TotalEntrantCount { get; init; }
    public required int FolloweeSignedUpCount { get; init; }
    public required int TotalSignedUpCount { get; init; }
    public required int FolloweeJoinedCount { get; init; }
    public required int TotalJoinedCount { get; init; }

    public required ZwiftEventSubgroupResponse[] EventSubgroups { get; init; }
    public required ZwiftEvenSeriesResponse? EventSeries { get; init; }

    public required string? AuxiliaryUrl { get; init; }
    public required string? ImageS3Name { get; init; }
    public required string? ImageS3Bucket { get; init; }
    public required string Sport { get; init; }
    public required string CullingType { get; init; }
    public required string[] RulesSet { get; init; }
    public required bool Recurring { get; init; }
    public required int? RecurringOffset { get; init; }
    public required bool PublishRecurring { get; init; }
    public required long? ParentId { get; init; }
    public required string Type { get; init; }
    public required long? WorkoutHash { get; init; }
    public required string? CustomUrl { get; init; }
    public required bool Restricted { get; init; }
    public required bool Unlisted { get; init; }
    public required string? EventSecret { get; init; }
    public required string? AccessExpression { get; init; }
    public required List<string> Tags { get; init; }
    public required int? LateJoinInMinutes { get; init; }
    public required object? TimeTrialOptions { get; init; }
    public required string? MicroserviceName { get; init; }
    public required string? MicroserviceExternalResourceId { get; init; }
    public required string? MicroserviceEventVisibility { get; init; }
    public required string? PartnerClientId { get; init; }
    public required int? MinGameVersion { get; init; }
    public required bool Recordable { get; init; }
    public required bool Imported { get; init; }
    public required object? EventTemplateId { get; init; }
    public required bool CategoryEnforcement { get; init; }
    public required string? RangeAccessLabel { get; init; }
    public required string EventType { get; init; }

    [JsonIgnore]
    public bool IsITT => RulesSet.Contains("NO_DRAFTING");
}

public sealed record ZwiftEventSubgroupResponse
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required int Label { get; init; }
    public required string? SubgroupLabel { get; init; }
    public required int RulesId { get; init; }
    public required int MapId { get; init; }
    public required long RouteId { get; init; }
    public required string? RouteUrl { get; init; }
    public required long? JerseyHash { get; init; }
    public required long? BikeHash { get; init; }
    public required int StartLocation { get; init; }
    public required List<long> InvitedLeaders { get; init; }
    public required List<long> InvitedSweepers { get; init; }
    public required int PaceType { get; init; }
    public required double FromPaceValue { get; init; }
    public required double ToPaceValue { get; init; }
    public required int? FieldLimit { get; init; }
    public required DateTime RegistrationStart { get; init; }
    public required DateTime RegistrationEnd { get; init; }
    public required DateTime LineUpStart { get; init; }
    public required DateTime LineUpEnd { get; init; }
    public required DateTime EventSubgroupStart { get; init; }
    public required int DurationInSeconds { get; init; }
    public required int Laps { get; init; }
    public required double DistanceInMeters { get; init; }
    public required bool SignedUp { get; init; }
    public required int SignupStatus { get; init; }
    public required bool Registered { get; init; }
    public required int RegistrationStatus { get; init; }
    public required int FolloweeEntrantCount { get; init; }
    public required int TotalEntrantCount { get; init; }
    public required int FolloweeSignedUpCount { get; init; }
    public required int TotalSignedUpCount { get; init; }
    public required int FolloweeJoinedCount { get; init; }
    public required int TotalJoinedCount { get; init; }
    public required string? AuxiliaryUrl { get; init; }
    public required string[] RulesSet { get; init; }
    public required long? WorkoutHash { get; init; }
    public required string? CustomUrl { get; init; }
    public required bool OverrideMapPreferences { get; init; }
    public required List<string> Tags { get; init; }
    public required int? LateJoinInMinutes { get; init; }
    public required object? TimeTrialOptions { get; init; }
    public required object? AccessValidationResult { get; init; }
    public required List<object> AccessRules { get; init; }
    public required string? RangeAccessLabel { get; init; }
}

public sealed record ZwiftEvenSeriesResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required bool Imported { get; init; }
}
