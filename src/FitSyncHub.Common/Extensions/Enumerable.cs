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
}
