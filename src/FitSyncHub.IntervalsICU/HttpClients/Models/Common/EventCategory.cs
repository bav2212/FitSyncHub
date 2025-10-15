using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<EventCategory>))]
public enum EventCategory
{
    Workout,
    RaceA,
    RaceB,
    RaceC,
    Note,
    Holiday,
    Sick,
    Injured,
    SetEftp,
    FitnessDays,
    SeasonStart,
    Target,
    SetFitness,
}
