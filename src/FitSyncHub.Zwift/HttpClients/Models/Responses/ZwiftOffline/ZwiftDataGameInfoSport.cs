using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;

namespace FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftOffline;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ZwiftDataGameInfoSport>))]
public enum ZwiftDataGameInfoSport
{
    Cycling,
    Running,
    Rowing,
}
