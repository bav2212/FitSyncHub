using System.Collections;

namespace FitSyncHub.Common.Collections;

public class FixedSizeQueue<T> : IReadOnlyCollection<T>, ICollection
{
    private readonly Queue<T> _queue;

    public FixedSizeQueue(int maxSize)
    {
        if (maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than zero.");
        }

        _queue = new Queue<T>(maxSize);
    }

    public void Enqueue(T item)
    {
        // Remove oldest if at capacity
        if (_queue.Count >= MaxSize)
        {
            _queue.Dequeue();
        }
        _queue.Enqueue(item);
    }

    public void Enqueue(params ICollection<T> items)
    {
        if (items.Count > MaxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(items), "Items length is greater than max size.");
        }

        foreach (var item in items)
        {
            Enqueue(item);
        }
    }

    public IEnumerator GetEnumerator() => _queue.GetEnumerator();
    void ICollection.CopyTo(Array array, int index) => (_queue as ICollection).CopyTo(array, index);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();
    public int Count => _queue.Count;
    public int MaxSize => _queue.Capacity;
    bool ICollection.IsSynchronized => ((ICollection)_queue).IsSynchronized;
    object ICollection.SyncRoot => ((ICollection)_queue).IsSynchronized;
}

