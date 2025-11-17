using System.Text.Json.Serialization;
using FitSyncHub.Zwift.Models;

namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public record ZwiftGameInfoMap
{
    public required string Name { get; init; }
    public required List<ZwiftGameInfoRoute> Routes { get; init; }
}
