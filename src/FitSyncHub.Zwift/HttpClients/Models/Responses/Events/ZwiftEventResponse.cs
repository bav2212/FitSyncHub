﻿namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

public record ZwiftEventResponse
{
    public required long Id { get; init; }
    public required int WorldId { get; init; }
    public required string Name { get; init; }
    public required long RouteId { get; init; }
    public required DateTime EventStart { get; init; }
    public required int Laps { get; init; }
    public required double DurationInSeconds { get; init; }
    public required double DistanceInMeters { get; init; }
    public required ZwiftEventSubgroupResponse[] EventSubgroups { get; init; }
    public required ZwiftEvenSeriesResponse? EventSeries { get; init; }
}

public record ZwiftEventSubgroupResponse
{
    public required int Id { get; init; }
    public required string? SubgroupLabel { get; init; }
    public required string[] RulesSet { get; init; }
}

public record ZwiftEvenSeriesResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required bool Imported { get; init; }
}
