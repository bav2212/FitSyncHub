using System.Xml;

namespace FitSyncHub.Common.Applications.Apple;

public static class AppleHealthExportFileParser
{
    public static List<AppleHeartRateRecord> LoadHeartRateData(string xmlFilePath)
    {
        List<AppleHeartRateRecord> heartRateData = [];
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true };

        using (var reader = XmlReader.Create(xmlFilePath, settings))
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Record"
                    && reader.GetAttribute("type") == "HKQuantityTypeIdentifierHeartRate")
                {
                    var timeString = reader.GetAttribute("startDate");
                    var valueString = reader.GetAttribute("value");

                    if (DateTime.TryParse(timeString, out var time) && int.TryParse(valueString, out var heartRate))
                    {
                        heartRateData.Add(new AppleHeartRateRecord
                        {
                            Time = time.ToUniversalTime(),
                            HeartRate = heartRate
                        });
                    }
                }
            }
        }

        return heartRateData;
    }

    public static int FindClosestHeartRate(List<AppleHeartRateRecord> heartRates, DateTime trackTime)
    {
        var closest = heartRates
            .OrderBy(hr => Math.Abs((hr.Time - trackTime).TotalSeconds)) // Find nearest timestamp
            .First();

        return closest.HeartRate;
    }
}

public record AppleHeartRateRecord
{
    public required DateTime Time { get; init; }
    public required int HeartRate { get; init; }
}
