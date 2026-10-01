using FitSyncHub.Zwift.Xml.Abstractions;
using FitSyncHub.Zwift.Xml.Models.Road;

namespace FitSyncHub.Zwift.Xml.Models;

public sealed record ZwiftXmlObjectRoadRoot : IZwiftXmlObjectRoot
{
    public required ZwiftXmlObjectRoadWorldElement World { get; init; }
}
