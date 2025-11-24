using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.JsonSerializerContexts.Converters;

public sealed class DateTimeWithoutColonOffsetJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffzzz";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();

        if (DateTimeOffset.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fffzzz",
                                         CultureInfo.InvariantCulture, DateTimeStyles.None,
                                         out var dto))
        {
            return dto.UtcDateTime;
        }

        // Try alternative format without a colon in offset
        if (DateTimeOffset.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fffzzzz",
                                         CultureInfo.InvariantCulture, DateTimeStyles.None,
                                         out dto))
        {
            return dto.UtcDateTime;
        }

        throw new JsonException($"Invalid DateTime format: {dateString}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
