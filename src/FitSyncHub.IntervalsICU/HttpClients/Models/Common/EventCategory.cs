using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverter<EventCategory>))]
public enum EventCategory
{
    WORKOUT,
    RACE_A,
    RACE_B,
    RACE_C,
    NOTE,
    HOLIDAY,
    SICK,
    INJURED,
    SET_EFTP,
    FITNESS_DAYS,
    SEASON_START,
    TARGET,
    SET_FITNESS,
}
