namespace FitSyncHub.GarminConnect.Models.Requests;

public sealed record GarminSetUserWeightRequest
{
    public required WeightUserData UserData { get; init; }
}

public sealed record WeightUserData
{
    public required double Weight { get; init; }
}
