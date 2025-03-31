
using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverter<WellnessMenstrualPhase>))]
public enum WellnessMenstrualPhase
{
    PERIOD,
    FOLLICULAR,
    OVULATING,
    LUTEAL,
    NONE
}
