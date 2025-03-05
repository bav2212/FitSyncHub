namespace FitSyncHub.GarminConnect.Models.Requests;

public record GarminSetUserWeightRequest
{
    public required WeightUserData UserData { get; init; }
}

public record WeightUserData
{
    public required double Weight { get; init; }
}
