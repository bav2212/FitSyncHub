
using System.Text.Json.Serialization;

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
