using Dynastream.Fit;
using DateTime = System.DateTime;

namespace FitSyncHub.Common;

public class TssCalculator
{
    public static CalculateTssResult? Calculate(FitMessages fitMessages, double ftp)
    {
        var powerPoints = fitMessages.RecordMesgs
            //For moving time for the whole activity Intervals.icu uses the speed if available with anything >= 0.05 m/s (0.18 km/h) considered moving. 
            .Where(x => x.GetSpeed() >= 0.05)
            .Select(x => new FitMessageRawPowerAndTime
            {
                Timestamp = x.GetTimestamp().GetDateTime(),
                Power = x.GetPower() ?? 0,
            })
            .ToList();

        if (powerPoints.Count == 0 || ftp <= 0)
        {
            return default;
        }

        return Calculate(powerPoints, ftp);
    }

    private static CalculateTssResult? Calculate(List<FitMessageRawPowerAndTime> fitData, double ftp)
    {
        // can't use timestamps, cause it could be pauses during ride
        var totalDuration = fitData.Count;
        var powerValues = fitData.Select(d => (double)d.Power).ToList();

        var np = CalculateNormalizedPower(powerValues);
        var intensityFactor = np / ftp;
        var tss = totalDuration * Math.Pow(intensityFactor, 2) / 3600 * 100;

        return new CalculateTssResult
        {
            NormalizedPower = np,
            IntensityFactor = intensityFactor,
            Tss = tss,
            Duration = totalDuration,
        };
    }

    private static double CalculateNormalizedPower(List<double> powerValues)
    {
        if (powerValues.Count < 30)
        {
            // If too few data points, fallback to average
            return powerValues.Average();
        }

        List<double> rollingAverages = [];
        for (var i = 0; i <= powerValues.Count - 30; i++)
        {
            rollingAverages.Add(powerValues.Skip(i).Take(30).Average());
        }

        var np = Math.Pow(rollingAverages.Average(p => Math.Pow(p, 4)), 0.25);
        return np;
    }

    private record FitMessageRawPowerAndTime
    {
        public required DateTime Timestamp { get; init; }
        public required ushort Power { get; init; }
    }
}

public record CalculateTssResult
{
    public required double NormalizedPower { get; init; }
    public required double IntensityFactor { get; init; }
    public required double Tss { get; init; }
    public required int Duration { get; init; }
}
