using System;
using System.Collections.Generic;
using Facepunch;
using System.Linq;

/// <summary>
/// This template is for implementing List<T> with Dispose that goes back to pool.
/// It exists to help with incorrect Dispose usage that leads to Pool leaks
/// </summary>
/// <example><code>sealed class PooledIntList : BasePooledList<int, PooledIntList> {}</code></example>
/// <typeparam name="T">Type of individual elemenents in a list</typeparam>
/// <typeparam name="SubclassT">CRTP parameter</typeparam>
public class BasePooledList<T, SubclassT> : List<T>, IDisposable, Pool.IPooled
    where SubclassT : BasePooledList<T, SubclassT>, new()
{
    void IDisposable.Dispose()
    {
        // Although this involves unbox.any, the perf is marginally as Unsafe.As<SubclassT>(this)
        // for 1 mill disposal calls (15ms vs 14ms). But we lose runtime type checking,
        // which can save us from complicated bugs
        SubclassT aliasVal = (SubclassT)this;
        Pool.Free(ref aliasVal);
    }

    void Pool.IPooled.EnterPool()
    {
        Clear();
    }

    void Pool.IPooled.LeavePool() { }
}

// Note: don't remove sealed - if you want to inherent from this, just inherit from BasePooledList
public sealed class PooledList<T> : BasePooledList<T, PooledList<T>>
{
}

public class PooledHashSet<T> : HashSet<T>, IDisposable, Pool.IPooled
{
    void IDisposable.Dispose()
    {
        PooledHashSet<T> copy = this;
        Pool.Free(ref copy);
    }
    
    void Pool.IPooled.EnterPool()
    {
        Clear();
    }
    
    void Pool.IPooled.LeavePool() {}
}

public struct PooledArray<T> : IDisposable, Pool.IPooled
{
    public readonly T this[int index]
    {
        get => Array[index];
        set => Array[index] = value;
    }
    public T[] Array { get; private set; }

    public PooledArray(int size)
    {
        Array = System.Buffers.ArrayPool<T>.Shared.Rent(size);
    }

    void IDisposable.Dispose()
    {
        if (Array != null)
        {
            System.Buffers.ArrayPool<T>.Shared.Return(Array, clearArray: true);
            Array = null;
        }
    }

    void Pool.IPooled.EnterPool()
    {
        
    }

    void Pool.IPooled.LeavePool() { }
    
    public static implicit operator T[](PooledArray<T> pooledArray)
    {
        return pooledArray.Array;
    }
}

public static class PooledArrayExtensions
{
    public static PooledArray<T> ToPooledArray<T>(this T[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        var pooledArray = new PooledArray<T>(array.Length);
        Array.Copy(array, pooledArray.Array, array.Length);
        return pooledArray;
    }
    
    public static PooledArray<T> ToPooledArray<T>(this List<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        var pooledArray = new PooledArray<T>(list.Count);
        list.CopyTo(pooledArray.Array);
        return pooledArray;
    }
    
    public static PooledArray<T> ToPooledArray<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
            throw new ArgumentNullException(nameof(enumerable));
        
        //double enumeration, rip.
        var pooledArray = new PooledArray<T>(enumerable.Count());
        int index = 0;
        foreach (var item in enumerable)
        {
            if (index >= pooledArray.Array.Length)
                throw new InvalidOperationException("The enumerable contains more items than the allocated array size.");

            pooledArray.Array[index++] = item;
        }
        return pooledArray;
    }
}
