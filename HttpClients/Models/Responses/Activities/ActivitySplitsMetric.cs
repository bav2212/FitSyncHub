namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;

public class ActivitySplitsMetric
{
    public float? Distance { get; init; }
    public int? ElapsedTime { get; init; }
    public float? ElevationDifference { get; init; }
    public int? MovingTime { get; init; }
    public int? Split { get; init; }
    public float? AverageSpeed { get; init; }
    public object? AverageGradeAdjustedSpeed { get; init; }
    public int? PaceZone { get; init; }
}
