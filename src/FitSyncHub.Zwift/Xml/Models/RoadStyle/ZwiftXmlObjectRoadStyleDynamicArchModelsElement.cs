using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.RoadStyle;


[XmlRoot("dynamicArchModels")]
public sealed class ZwiftXmlObjectRoadStyleDynamicArchModelsElement
{
    [XmlElement("startArchGde")]
    public ZwiftXmlObjectRoadStyleArchModelElement? StartArchGde { get; set; }

    [XmlElement("finishArchGde")]
    public ZwiftXmlObjectRoadStyleArchModelElement? FinishArchGde { get; set; }

    [XmlElement("checkpointArchGde")]
    public ZwiftXmlObjectRoadStyleArchModelElement? CheckpointArchGde { get; set; }
}

public sealed class ZwiftXmlObjectRoadStyleArchModelElement
{
    [XmlAttribute("model")]
    public string? Model { get; set; }
}
