namespace FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;

// that's not all fields, extend if needed
public sealed record ZwiftRacingEventResponse
{
    public required string Name { get; init; }
    public required List<ZwiftRacingEventRiderResponse> Riders { get; init; }
}

public sealed record ZwiftRacingEventRiderResponse
{
    public int? RiderId { get; init; }
    public string? Name { get; init; }
    public ZwiftRacingEventRiderRace? Race { get; init; }
    //public ZwiftRacingPowerData? Power { get; init; }
    public ZwiftRacingEventRiderClub? Club { get; init; }
}

public sealed record ZwiftRacingEventRiderRace
{
    public double? Rating { get; init; }
    public int? Finishes { get; init; }
    public int? Wins { get; init; }
    public int? Podiums { get; init; }
    public int? Dnfs { get; init; }
    public double? Max30 { get; init; }
}


public sealed record ZwiftRacingEventRiderClub
{
    public int? ClubId { get; init; }
    public string? Name { get; init; }
}

