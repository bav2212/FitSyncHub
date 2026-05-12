using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "homedata")]
public sealed class ZwiftXmlObjectRouteHomedataElement
{
    [XmlAttribute(AttributeName = "difficulty")]
    public double Difficulty { get; set; }
    [XmlAttribute(AttributeName = "duration")]
    public int Duration { get; set; }
    [XmlAttribute(AttributeName = "xp")]
    public uint Xp { get; set; }
    [XmlAttribute(AttributeName = "bikeRec")]
    public bool BikeRec { get; set; }
    [XmlAttribute(AttributeName = "workoutRec")]
    public bool WorkoutRec { get; set; }
    [XmlAttribute(AttributeName = "bikeType")]
    public uint BikeType { get; set; }
    [XmlAttribute(AttributeName = "runRec")]
    public bool RunRec { get; set; }
    [XmlAttribute(AttributeName = "publishedOn")]
    public string? PublishedOn { get; set; }
    [XmlAttribute(AttributeName = "rowRec")]
    public bool RowRec { get; set; }
    [XmlAttribute(AttributeName = "speedScaledDistanceInMeters")]
    public double SpeedScaledDistanceInMeters { get; set; }
    [XmlAttribute(AttributeName = "speedScaledAscentInMeters")]
    public double SpeedScaledAscentInMeters { get; set; }
    [XmlAttribute(AttributeName = "speedScaledLeadinDistanceInMeters")]
    public double SpeedScaledLeadinDistanceInMeters { get; set; }
    [XmlAttribute(AttributeName = "speedScaledLeadinAscentInMeters")]
    public double SpeedScaledLeadinAscentInMeters { get; set; }
    [XmlAttribute(AttributeName = "speedScaledFreeRideLeadinDistanceInMeters")]
    public double SpeedScaledFreeRideLeadinDistanceInMeters { get; set; }
    [XmlAttribute(AttributeName = "speedScaledFreeRideLeadinAscentInMeters")]
    public double SpeedScaledFreeRideLeadinAscentInMeters { get; set; }
    [XmlAttribute(AttributeName = "speedScaledMeetupLeadinDistanceInMeters")]
    public double SpeedScaledMeetupLeadinDistanceInMeters { get; set; }
    [XmlAttribute(AttributeName = "speedScaledMeetupLeadinAscentInMeters")]
    public double SpeedScaledMeetupLeadinAscentInMeters { get; set; }
}
