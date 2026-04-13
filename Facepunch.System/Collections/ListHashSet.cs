using System.Collections;
using System.Collections.Generic;

public class ListHashSet<T> : IEnumerable<T>, IList<T>
{
	private Dictionary<T, int> val2idx;
	private Dictionary<int, T> idx2val;

	private BufferList<T> vals;

    public ListHashSet()
        : this(8)
    {
    }

	public ListHashSet(int capacity)
	{
		val2idx = new Dictionary<T, int>(capacity);
		idx2val = new Dictionary<int, T>(capacity);

		vals = new BufferList<T>(capacity);
	}

	public void Add(T val)
	{
		int idx = vals.Count;

		val2idx.Add(val, idx);
		idx2val.Add(idx, val);

		vals.Add(val);
	}

    public bool TryAdd(T val)
    {
        if (Contains(val)) return false;
        Add(val);
        return true;
    }

	public void AddRange(List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
            Add(list[i]);
		}
	}

    public void AddRange(IEnumerable<T> enumerable)
    {
        foreach(T item in enumerable)
        {
            Add(item);
        }
    }

	public bool Contains(T val)
	{
		return val2idx.ContainsKey(val);
	}

	public bool Remove(T val)
	{
		int idx;
		if (!val2idx.TryGetValue(val, out idx)) return false;
		Remove(idx, val);
		return true;
	}

	public void RemoveAt(int idx)
	{
		T val;
		if (!idx2val.TryGetValue(idx, out val)) return;
		Remove(idx, val);
	}

	public int IndexOf(T item)
	{
		int idx;
		if (!val2idx.TryGetValue(item, out idx)) return -1;
		return idx;
	}

	public void ReplaceAt(int index, T item)
	{
		var oldItem = vals[index];

		val2idx.Remove(oldItem);
		val2idx.Add(item, index);

		vals[index] = item;
		idx2val[index] = item;
	}
	
	public void Insert(int index, T item)
	{
		vals.Add(default);
		
		for (int i = vals.Count - 1; i > index; i--)
		{
			var shifted = vals[i - 1];
			vals[i] = shifted;

			val2idx[shifted] = i;
			idx2val[i] = shifted; 
		}

		vals[index] = item;
		val2idx[item] = index;
		idx2val[index] = item;
	}

	public void Clear()
	{
		if (Count == 0) return;

		val2idx.Clear();
		idx2val.Clear();
		vals.Clear();
	}

	private void Remove(int idx_remove, T val_remove)
	{
		int idx_update = vals.Count-1;
		var val_update = idx2val[idx_update];

		// Remove value
		vals.RemoveUnordered(idx_remove);

		// Update moved value in the lookup dictionaries
		val2idx[val_update] = idx_remove;
		idx2val[idx_remove] = val_update;

		// Remove outdated value from the lookup dictionaries
		val2idx.Remove(val_remove);
		idx2val.Remove(idx_update);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		for (int i = 0; i < vals.Count; i++)
		{
			array[arrayIndex + i] = vals[i];
		}
	}

	public BufferList<T> Values
	{
		get { return vals; }
	}

	public int Count
	{
		get { return vals.Count; }
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public T this[int index]
	{
		 get { return vals[index];  }
		 set { vals[index] = value; }
	}

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly BufferList<T> list;
		private int index;

        public Enumerator(ListHashSet<T> set)
        {
            list = set.vals;
            index = -1; // need a MoveNext to get first value
        }

        public bool MoveNext()
        {
            index++;
            return index < list.Count;
        }

        public void Reset() => index = -1;

        public T Current => list[index];

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

    /// <summary>
    /// Compare two collections and categorize the difference into 3 lists - added, removed and remained
    /// Note: Assumes that a and b don't have duplicates!
    /// </summary>
    public static void Compare(ListHashSet<T> a, ListHashSet<T> b, List<T> added, List<T> removed, List<T> remained)
    {
        //
        // Technically all this code does is this
        //
        // added.AddRange( b.Where( x => !a.Contains( x ) ) );
        // removed.AddRange( a.Where( x => !b.Contains( x ) ) );
        // remained.AddRange( a.Where( x => b.Contains( x ) ) );
        //
        // But in a slightly faster way

        // Both null or empty
        if (a == null && b == null) return;

        // a is null, then everything is new
        if (a == null)
        {
            added?.AddRange(b);
            return;
        }

        // b is null, then everything is removed
        if (b == null)
        {
            removed?.AddRange(a);
            return;
        }

        if (a.Count == 0 && b.Count == 0) return;

        HashSet<T> alreadyProcessed = Facepunch.Pool.Get<HashSet<T>>();
        foreach (var objB in b)
        {
            if (alreadyProcessed.Contains(objB))
            {
                continue;
            }

            if (a.Contains(objB))
            {
                remained?.Add(objB);
            }
            else
            {
                added?.Add(objB);
            }
            alreadyProcessed.Add(objB);
        }

        foreach (var objA in a)
        {
            if (alreadyProcessed.Contains(objA))
            {
                continue;
            }

            if (b.Contains(objA))
            {
                remained?.Add(objA);
            }
            else
            {
                removed?.Add(objA);
            }
            alreadyProcessed.Add(objA);
        }

        Facepunch.Pool.FreeUnmanaged(ref alreadyProcessed);
    }
    
    /// <summary>
    /// Compare two collections partially and categorize the difference into 3 lists - added, removed and remained
    /// The comparison stops when added reached addedLimit and removed reaches removedLimit
    /// Note: Assumes that a and b don't have duplicates!
    /// </summary>
    public static void PartialCompare(ListHashSet<T> a, ListHashSet<T> b, List<T> added, int addedLimit, List<T> removed, int removedLimit, List<T> remained)
    {
        //
        // Technically all this code does is this
        //
        // added.AddRange( b.Where( x => !a.Contains( x ) ) );
        // removed.AddRange( a.Where( x => !b.Contains( x ) ) );
        // remained.AddRange( a.Where( x => b.Contains( x ) ) );
        //
        // But in a slightly faster way

        // Both null or empty
        if (a == null && b == null) return;

        if(addedLimit == 0)
        {
            added = null;
        }
        if(removedLimit == 0)
        {
            removed = null;
        }

        // a is null, then everything is new
        if (a == null)
        {
            if(added != null)
            {
                for (int i = 0; i < addedLimit; i++)
                {
                    added.Add(b[i]);
                }
            }
            return;
        }

        // b is null, then everything is removed
        if (b == null)
        {
            if(removed != null)
            {
                for (int i = 0; i < removedLimit; i++)
                {
                    removed.Add(a[i]);
                }
            }
            return;
        }

        if (a.Count == 0 && b.Count == 0) return;

        HashSet<T> alreadyProcessed = Facepunch.Pool.Get<HashSet<T>>();
        foreach (var objB in b)
        {
            if (alreadyProcessed.Contains(objB))
            {
                continue;
            }

            bool shouldStop = false;
            if (a.Contains(objB))
            {
                remained?.Add(objB);
            }
            else if(added != null)
            {
                added.Add(objB);
                shouldStop = --addedLimit <= 0;
            }
            alreadyProcessed.Add(objB);
            if(shouldStop)
            {
                break;
            }
        }

        foreach (var objA in a)
        {
            if (alreadyProcessed.Contains(objA))
            {
                continue;
            }

            if (b.Contains(objA))
            {
                remained?.Add(objA);
            }
            else if(removed != null)
            {
                removed.Add(objA);
                if (--removedLimit <= 0)
                {
                    break;
                }
            }
            alreadyProcessed.Add(objA);
        }

        Facepunch.Pool.FreeUnmanaged(ref alreadyProcessed);
    }


    public T GetRandom()
    {
	    int count = vals.Count;
	    if (count == 0)
	    {
		    return default(T);
	    }
	    
	    return vals[Random.Shared.Next(0, count)];
    }
}
