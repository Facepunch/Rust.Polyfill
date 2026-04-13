using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// What the fuck, right?
// The reason this exists is because iterating over a list is twice as slow as iterating over an array
// This can make a significant difference for things that regularly iterate over long lists
// With this they can simply iterate over the buffer array from zero to count with no overhead
//

public class BufferList<T> : IEnumerable<T>
{
    public int Count
	{
		get { return count; }
	}

	public int Capacity
	{
		get { return buffer.Length; }
	}

	public T[] Buffer
	{
		get { return buffer; }
	}

	public T this[int index]
	{
		 get { return buffer[index];  }
		 set { buffer[index] = value; }
	}

	private int count;
	private T[] buffer;

	public BufferList()
	{
		buffer = Array.Empty<T>();
	}

	public BufferList(int capacity)
	{
		buffer = capacity == 0 ? Array.Empty<T>() : new T[capacity];
	}

	protected BufferList(T[] array)
	{
		buffer = array;
	}

	public void Push(T element)
	{
		Add(element);
	}

	public T Pop()
	{
		if (count == 0) return default(T);

		var res = buffer[count - 1];

		buffer[count - 1] = default(T);
		count--;

		return res;
	}

	public void Add(T element)
	{
		if (count == buffer.Length) Resize(Math.Max(buffer.Length * 2, 8));

		buffer[count] = element;
		count++;
	}

    public void AddRange(BufferList<T> elements)
    {
        if (count + elements.count > buffer.Length) Resize(Math.Max(count + elements.count, 8));

        Array.Copy(elements.buffer, 0, buffer, count, elements.count);
        count += elements.count;
    }
	
	public void AddSpan(Span<T> span)
	{
		count += span.Length;
		if (buffer.Length < count) Resize(Mathf.NextPowerOfTwo(count));
		span.CopyTo(buffer.AsSpan(count - span.Length));
	}

    public void AddSpan(ReadOnlySpan<T> span)
    {
        count += span.Length;
        if (buffer.Length < count) Resize(Mathf.NextPowerOfTwo(count));
        span.CopyTo(buffer.AsSpan(count - span.Length));
    }

    public Span<T> NewSpan(int length)
	{
		count += length;
		if (buffer.Length < count) Resize(Mathf.NextPowerOfTwo(count));
		return buffer.AsSpan(count - length);
	}

	public bool Remove(T element)
	{
		var index = Array.IndexOf(buffer, element);

		if (index == -1) return false;
        if (count == 0) return false;

		RemoveAt(index);
		return true;
	}

	public void RemoveAt(int index)
	{
		for (int i = index; i < count - 1; i++)
		{
			buffer[i] = buffer[i + 1];
		}

		buffer[count - 1] = default(T);
		count--;
	}
	
	public bool RemoveUnordered(T element)
	{
		var index = Array.IndexOf(buffer, element);
		
		if (index == -1) return false;
		if (count == 0) return false;

		RemoveUnordered(index);
		return true;
	}

	public void RemoveUnordered(int index)
	{
		buffer[index] = buffer[count - 1];

		buffer[count - 1] = default(T);
		count--;
	}

    public void CopyFrom(T[] array)
    {
        int newLength = array.Length;
        if (newLength > buffer.Length)
        {
            Resize(newLength);
        }
        Array.Copy(array, buffer, newLength);
        if(newLength < count)
        {
            Array.Clear(buffer, newLength, count - newLength);
        }
        count = newLength;
    }

    public void CopyFrom(List<T> list)
    {
        int newLength = list.Count;
        if(newLength > buffer.Length)
        {
            Resize(newLength);
        }
        list.CopyTo(buffer);
        if (newLength < count)
        {
            Array.Clear(buffer, newLength, count - newLength);
        }
        count = newLength;
    }

	public int IndexOf(T element)
	{
		return Array.IndexOf(buffer, element);
	}

	public int LastIndexOf(T element)
	{
		return Array.LastIndexOf(buffer, element);
	}

	public bool Contains(T element)
	{
		return Array.IndexOf(buffer, element) != -1;
	}

	public void Clear()
	{
		if (count == 0) return;

		Array.Clear(buffer, 0, count);
		count = 0;
    }

    public void Resize(int newSize)
    {
        Array.Resize(ref buffer, newSize);
    }

    // Returns a read-only span over number of items in the buffer,
    // not full buffer!
    public ReadOnlySpan<T> ContentReadOnlySpan()
    {
        return new(Buffer, 0, count);
    }

    // Returns a span over number of items in the buffer,
    // not full buffer!
    public Span<T> ContentSpan()
    {
        return new(Buffer, 0, count);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly BufferList<T> list;
        private int index;

        public Enumerator(BufferList<T> list)
        {
            this.list = list;
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
}
