using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
public enum EventType
{
    Ride,
    Workout
}

