using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;

namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ZwiftGameInfoSport>))]
public enum ZwiftGameInfoSport
{
    Cycling,
    Running,
    Rowing,
}
