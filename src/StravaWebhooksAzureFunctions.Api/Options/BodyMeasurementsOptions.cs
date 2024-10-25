namespace StravaWebhooksAzureFunctions.Options;

public record BodyMeasurementsOptions
{
    public const string Position = "BodyMeasurements";
    public required string VerifyToken { get; init; }
}
