using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<EventSubType>))]
public enum EventSubType
{
    None,
    Commute,
    Warmup,
    Cooldown,
}
