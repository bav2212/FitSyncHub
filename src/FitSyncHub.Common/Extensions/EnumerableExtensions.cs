namespace FitSyncHub.Common.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        foreach (var item in source)
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : struct
    {
        foreach (var item in source)
        {
            if (item.HasValue)
            {
                yield return item.Value;
            }
        }
    }


    public static T? FirstOrNull<T>(this IEnumerable<T> values, Func<T, bool> predicate)
        where T : struct
    {
        foreach (var v in values)
        {
            if (predicate(v))
            {
                return v;
            }
        }
        return null;
    }

    public static T? FirstOrNull<T>(this IEnumerable<T> values)
      where T : struct
    {
        return FirstOrNull(values, _ => true);
    }
}
