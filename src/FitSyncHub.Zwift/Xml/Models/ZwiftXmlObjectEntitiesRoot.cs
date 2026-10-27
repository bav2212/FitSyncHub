using FitSyncHub.Zwift.Xml.Abstractions;
using FitSyncHub.Zwift.Xml.Models.Entities;

namespace FitSyncHub.Zwift.Xml.Models;

public sealed record ZwiftXmlObjectEntitiesRoot : IZwiftXmlObjectRoot
{
    public required ZwiftXmlObjectEntitiesVersionElement Version { get; init; }
    public required ZwiftXmlObjectEntitiesWorldElement World { get; init; }
}
