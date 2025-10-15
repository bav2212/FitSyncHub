
using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<WellnessMenstrualPhase>))]
public enum WellnessMenstrualPhase
{
    Period,
    Follicular,
    Ovulating,
    Luteal,
    None,
}
