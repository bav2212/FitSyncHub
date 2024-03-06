using System.Text.Json;

namespace StravaWebhooksAzureFunctions;

public class Constants
{
    public const long MyAthleteId = 50156776;

    public readonly static JsonSerializerOptions StravaApiJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}
