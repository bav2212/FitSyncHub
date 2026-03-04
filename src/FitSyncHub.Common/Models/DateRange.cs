namespace FitSyncHub.Common.Models;

public sealed record DateRange(DateTime From, DateTime To)
{
    public static IEnumerable<DateRange> GetDateRanges(
        DateTime from,
        DateTime to,
        TimeSpan rangeBy)
    {
        // do not delete this cause it will create an infinite loop and cause a memory leak
        if (rangeBy >= TimeSpan.Zero)
        {
            throw new ArgumentException($"'{nameof(rangeBy)}' should be less than zero");
        }

        if (to <= from)
        {
            throw new ArgumentException($"'{nameof(to)}' should be greater than or equal to '{nameof(from)}'");
        }

        while (true)
        {
            var fromRangeValue = to + rangeBy;
            if (fromRangeValue < from)
            {
                yield return new DateRange(from, to);
                yield break;
            }

            yield return new DateRange(fromRangeValue, to);
            to = fromRangeValue;
        }
    }
}
