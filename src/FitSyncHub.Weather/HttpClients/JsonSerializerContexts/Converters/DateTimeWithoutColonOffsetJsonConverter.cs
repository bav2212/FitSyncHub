using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitSyncHub.Weather.HttpClients.JsonSerializerContexts.Converters;

public class GMTDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // hack, but it works, because the OpenMeteo API returns UTC times without an offset, and we want to treat them as UTC
        var dateTime = reader.GetDateTime();
        return new DateTimeOffset(dateTime, TimeSpan.Zero);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        // Always write in UTC with Z suffix
        writer.WriteStringValue(value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
    }
}
