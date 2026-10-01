using FitSyncHub.Zwift.Xml.Abstractions;
using FitSyncHub.Zwift.Xml.Models.RoadStyle;

namespace FitSyncHub.Zwift.Xml.Models;

public sealed record ZwiftXmlObjectRoadStyleRoot : IZwiftXmlObjectRoot
{
    public required ZwiftXmlObjectRoadStyleVersionElement Version { get; init; }
    public required ZwiftXmlObjectRoadStyleRoadsElement Roads { get; init; }
    public ZwiftXmlObjectRoadStyleDynamicArchModelsElement? DynamicArchModels { get; init; }
}
