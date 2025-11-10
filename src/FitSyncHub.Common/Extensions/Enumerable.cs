namespace FitSyncHub.Common.Extensions;

public static class Enumerable
{
    extension<TSource>(IEnumerable<TSource?> source) where TSource : class
    {
        public IEnumerable<TSource> WhereNotNull()
        {
            foreach (var item in source)
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }
    }

    extension<TSource>(IEnumerable<TSource?> source) where TSource : struct
    {
        public IEnumerable<TSource> WhereNotNull()
        {
            foreach (var item in source)
            {
                if (item.HasValue)
                {
                    yield return item.Value;
                }
            }
        }
    }

    extension<TSource>(IEnumerable<TSource> source) where TSource : struct
    {
        public TSource? FirstOrNull(Func<TSource, bool> predicate)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
            return null;
        }

        public TSource? FirstOrNull()
        {
            return source.FirstOrNull(_ => true);
        }
    }
}
