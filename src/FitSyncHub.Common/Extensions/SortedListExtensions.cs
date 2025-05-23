using System.Numerics;

namespace FitSyncHub.Common.Extensions;

public static class SortedListExtensions
{
    public static TValue GetClosestValue<TKey, TValue, TDiffResult>(
        this SortedList<TKey, TValue> sortedList,
        TKey target,
        Func<TKey, TKey, TDiffResult> diffFunc)
        where TKey : notnull, IComparable<TKey>
        where TDiffResult : INumber<TDiffResult>
    {
        if (sortedList.Count == 0)
        {
            throw new InvalidOperationException("List is empty");
        }

        var keys = sortedList.Keys;
        var left = 0;
        var right = keys.Count - 1;

        while (left <= right)
        {
            var mid = (left + right) / 2;
            var cmp = keys[mid].CompareTo(target);

            if (cmp == 0)
            {
                return sortedList[keys[mid]];
            }
            else if (cmp < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        // Now left is the insertion point
        if (left == 0)
        {
            return sortedList[keys[0]];
        }

        if (left == keys.Count)
        {
            return sortedList[keys[^1]];
        }

        // Compare the two neighbors to find the closest
        var before = keys[left - 1];
        var after = keys[left];

        var diffBefore = TDiffResult.Abs(diffFunc(target, before));
        var diffAfter = TDiffResult.Abs(diffFunc(after, target));

        return diffBefore <= diffAfter ? sortedList[before] : sortedList[after];
    }
}
