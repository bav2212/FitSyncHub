using FitSyncHub.Zwift.Xml.Abstractions;
using FitSyncHub.Zwift.Xml.Models.Route;

namespace FitSyncHub.Zwift.Xml.Models;

public sealed record ZwiftXmlObjectRouteRoot : IZwiftXmlObjectRoot
{
    public required ZwiftXmlObjectRouteRouteElement Route { get; init; }
    public ZwiftXmlObjectRouteHomedataElement? Homedata { get; init; }
}
