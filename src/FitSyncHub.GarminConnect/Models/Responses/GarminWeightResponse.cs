namespace FitSyncHub.GarminConnect.Models.Responses;

public record GarminWeightResponse
{
    public required string StartDate { get; init; }
    public required string EndDate { get; init; }
    public required DateWeightList[] DateWeightList { get; init; }
    public required TotalAverage TotalAverage { get; init; }
}

public record TotalAverage
{
    public required long From { get; init; }
    public required long Until { get; init; }
    public float? Weight { get; init; }
    public float? Bmi { get; init; }
    public float? BodyFat { get; init; }
    public float? BodyWater { get; init; }
    public int? BoneMass { get; init; }
    public int? MuscleMass { get; init; }
    public float? PhysiqueRating { get; init; }
    public float? VisceralFat { get; init; }
    public float? MetabolicAge { get; init; }
}

public record DateWeightList
{
    public required long SamplePk { get; init; }
    public required long Date { get; init; }
    public required string CalendarDate { get; init; }
    public required float Weight { get; init; }
    public required float? Bmi { get; init; }
    public required float? BodyFat { get; init; }
    public required float? BodyWater { get; init; }
    public int? BoneMass { get; init; }
    public int? MuscleMass { get; init; }
    public float? PhysiqueRating { get; init; }
    public float? VisceralFat { get; init; }
    public float? MetabolicAge { get; init; }
    public required string SourceType { get; init; }
    public required long TimestampGMT { get; init; }
    public required float WeightDelta { get; init; }
}

public class GarminWeightResponseComparer : IEqualityComparer<GarminWeightResponse>
{
    public bool Equals(GarminWeightResponse? x, GarminWeightResponse? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.StartDate == y.StartDate &&
               x.EndDate == y.EndDate &&
               x.TotalAverage.Equals(y.TotalAverage) &&
               x.DateWeightList.SequenceEqual(y.DateWeightList);
    }

    public int GetHashCode(GarminWeightResponse obj)
    {
        if (obj == null)
        {
            return 0;
        }

        var hash = HashCode.Combine(obj.StartDate, obj.EndDate, obj.TotalAverage);
        foreach (var item in obj.DateWeightList)
        {
            hash = HashCode.Combine(hash, item);
        }

        return hash;
    }
}
