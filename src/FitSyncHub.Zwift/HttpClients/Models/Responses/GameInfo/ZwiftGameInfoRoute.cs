﻿using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public record ZwiftGameInfoRoute
{
    public required string Name { get; init; }
    public required long Id { get; init; }
    public required double DistanceInMeters { get; init; }
    public required double AscentInMeters { get; init; }
    public required string LocKey { get; init; }
    public required int LevelLocked { get; init; }
    public required bool PublicEventsOnly { get; init; }
    public required bool SupportedLaps { get; init; }
    public required double LeadinAscentInMeters { get; init; }
    public required double LeadinDistanceInMeters { get; init; }
    public required int BlockedForMeetups { get; init; }
    public required int Xp { get; init; }
    public required int Duration { get; init; }
    public required double Difficulty { get; init; }
    public required IReadOnlyList<ZwiftGameInfoSport> Sports { get; init; }

    [JsonIgnore]
    public double TotalDistanceInMeters => DistanceInMeters + LeadinAscentInMeters;
    [JsonIgnore]
    public double TotalAscentInMeters => AscentInMeters + LeadinAscentInMeters;
}
