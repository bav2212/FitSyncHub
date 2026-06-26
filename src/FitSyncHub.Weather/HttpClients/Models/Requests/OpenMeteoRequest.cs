using FitSyncHub.Common.Models;

namespace FitSyncHub.Weather.HttpClients.Models.Requests;

public sealed record OpenMeteoRequest
{
    public required Coordinate Coordinate { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
}
