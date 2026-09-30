using FitSyncHub.Zwift.Xml.Abstractions;
using FitSyncHub.Zwift.Xml.Models.Route;

namespace FitSyncHub.Zwift.Xml.Models;

public sealed record ZwiftXmlObjectRouteRoot : IZwiftXmlObjectRoot
{
    public ZwiftXmlObjectRouteVersionElement? Version { get; init; }
    public required ZwiftXmlObjectRouteRouteElement Route { get; init; }
    public ZwiftXmlObjectRouteSegmentDataElement? SegmentData { get; init; }
    public ZwiftXmlObjectRouteLeadInHighResCheckpointElement? LeadInHighResCheckpoint { get; init; }
    public ZwiftXmlObjectRouteLeadInCheckpointElement? LeadInCheckpoint { get; init; }
    public ZwiftXmlObjectRouteLevelEditorInfoElement? LevelEditorInfo { get; init; }
    public ZwiftXmlObjectRouteDecisionElement? Decision { get; init; }
    public ZwiftXmlObjectRouteHighResCheckpointElement? HighResCheckpoint { get; init; }
    public ZwiftXmlObjectRouteHomedataElement? Homedata { get; init; }
    public ZwiftXmlObjectRouteEnableDynamicArchesStartElement? EnableDynamicArchesStart { get; init; }
    public ZwiftXmlObjectRouteEnableDynamicArchesEndElement? EnableDynamicArchesEnd { get; init; }
    public ZwiftXmlObjectRouteSpawnAreaElement? SpawnArea { get; init; }
    public ZwiftXmlObjectRoutePrivateSpawnAreaElement? PrivateSpawnArea { get; init; }
    public ZwiftXmlObjectRouteCheckpointElement? Checkpoint { get; init; }
}
