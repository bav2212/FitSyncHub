using System.Xml;

namespace FitSyncHub.Common.Applications.Apple;

public static class AppleHealthExportFileParser
{
    public static SortedList<DateTime, AppleHeartRateRecord> LoadHeartRateData(string xmlFilePath,
        DateTime from,
        DateTime? to = default)
    {
        SortedList<DateTime, AppleHeartRateRecord> heartRateData = [];
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
                        var timeUtc = time.ToUniversalTime();
                        if (timeUtc < from)
                        {
                            continue;
                        }

                        if (to.HasValue && timeUtc > to)
                        {
                            break;
                        }

                        heartRateData.Add(timeUtc, new AppleHeartRateRecord
                        {
                            Time = timeUtc,
                            HeartRate = heartRate
                        });
                    }
                }
            }
        }

        return heartRateData;
    }
}

public record AppleHeartRateRecord
{
    public required DateTime Time { get; init; }
    public required int HeartRate { get; init; }
}
