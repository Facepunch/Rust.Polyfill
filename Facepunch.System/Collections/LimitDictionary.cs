using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LimitDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    public int MaxCapacity = 8192;

    private Queue<TKey> entries;

    public LimitDictionary(int maxCapacity = 8192)
    {
        MaxCapacity = maxCapacity;
        entries = new Queue<TKey>(MaxCapacity);
    }

    public new void TryAdd(TKey key, TValue item)
    {
        if (ContainsKey(key)) return;

        if (Count == MaxCapacity)
        {
            var oldest = entries.Dequeue();
            Remove(oldest);
        }

        Add(key, item);
        entries.Enqueue(key);
    }
}
