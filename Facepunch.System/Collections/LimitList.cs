using System.Collections.Generic;

public class LimitList<T> : List<T>
{
    public int maxCapacity;

    private Queue<T> entries;

    public LimitList(int maxCapacity = 4096)
    {
        this.maxCapacity = maxCapacity;
        entries = new Queue<T>(this.maxCapacity);
    }

    public new void Add(T item)
    {
        while (Count > maxCapacity) // Handle instances where multiple entries are inserted at once (i.e. AddRange)
        {
            var oldest = entries.Dequeue();
            Remove(oldest);
        }

        base.Add(item);
        entries.Enqueue( item );
    }

    public new void AddRange(IEnumerable<T> items)
    {
        foreach ( var item in items )
        {
            Add(item);
        }
    }
}

