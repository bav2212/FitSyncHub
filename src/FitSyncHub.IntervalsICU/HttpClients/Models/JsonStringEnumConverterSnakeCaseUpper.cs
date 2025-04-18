using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models;
public class JsonStringEnumConverterSnakeCaseUpper<TEnum> : JsonStringEnumConverter<TEnum>
    where TEnum : struct, Enum
{
    public JsonStringEnumConverterSnakeCaseUpper() : base(
        namingPolicy: JsonNamingPolicy.SnakeCaseUpper,
        allowIntegerValues: true)
    {
    }
}
