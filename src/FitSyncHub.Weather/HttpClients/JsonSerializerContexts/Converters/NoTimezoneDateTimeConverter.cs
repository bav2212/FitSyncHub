using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitSyncHub.Weather.HttpClients.JsonSerializerContexts.Converters;

public class NoTimezoneDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss"; // No timezone

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Parse as unspecified kind
        if (DateTime.TryParse(reader.GetString(), out var dt))
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        }
        throw new JsonException("Invalid date format");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Force unspecified kind and format without timezone
        var unspecified = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        writer.WriteStringValue(unspecified.ToString(Format));
    }
}
