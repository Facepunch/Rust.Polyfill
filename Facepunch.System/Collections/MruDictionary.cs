using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

// Reference:
// http://www.informit.com/guides/content.aspx?g=dotnet&seqNum=626

public class MruDictionary<Key, Value> : IEnumerable<KeyValuePair<Key, Value>>
{
	private int capacity;
	private Queue<LinkedListNode<KeyValuePair<Key, Value>>> recycled;
	private LinkedList<KeyValuePair<Key, Value>> list;
	private Dictionary<Key, LinkedListNode<KeyValuePair<Key, Value>>> dict;
    private Action<Key, Value> valueRecycler;

    public int Capacity => capacity;
	public int Count => list.Count;
    public long EvictionCount { get; private set; } // Number of times a value was removed due to capacity being reached

    public MruDictionary( int capacity, Action<Key, Value> valueRecycler = null )
	{
		this.capacity = capacity;

		list = new LinkedList<KeyValuePair<Key, Value>>();
		dict = new Dictionary<Key, LinkedListNode<KeyValuePair<Key, Value>>>( capacity );
		recycled = new Queue<LinkedListNode<KeyValuePair<Key, Value>>>( capacity );
        this.valueRecycler = valueRecycler;

		for ( int i = 0; i < capacity; i++ )
        { 
            recycled.Enqueue( new LinkedListNode<KeyValuePair<Key, Value>>( default ) );
		}
	}

	public void Add( Key key, Value value )
	{
		if ( dict.ContainsKey( key ) )
            throw new InvalidOperationException( "An item with the same key has already been added." );

		if ( dict.Count >= capacity )
			RemoveLast();

		var node = recycled.Count > 0
			? recycled.Dequeue()
            : new LinkedListNode<KeyValuePair<Key, Value>>( default ); // this shouldn't happen, but just in case

        node.Value = new KeyValuePair<Key, Value>( key, value );

		list.AddFirst( node );
		dict.Add( key, node );
	}

    public void Remove( Key key )
    {
        if ( dict.TryGetValue( key, out var node ) )
        {
            Assert.AreEqual( key, node.Value.Key );
            var value = node.Value.Value;

			list.Remove( node );
            dict.Remove( key );

            valueRecycler?.Invoke( key, value );
            node.Value = default;
            recycled.Enqueue( node );
        }
    }

	private void RemoveLast()
    {
		Assert.IsTrue( list.Count > 0 );

        var lastEntry = list.Last;
        Assert.IsTrue( dict.Remove( lastEntry.Value.Key ) );
        list.RemoveLast();

		valueRecycler?.Invoke( lastEntry.Value.Key, lastEntry.Value.Value );
        lastEntry.Value = default;
		recycled.Enqueue( lastEntry );

        EvictionCount++;
    }

	public bool TryGetValue( Key key, out Value value )
	{
		if ( dict.TryGetValue( key, out var node ) )
		{
			Assert.AreEqual( key, node.Value.Key );
			value = node.Value.Value;

            // move this node to the front of the list
            list.Remove( node );
            list.AddFirst( node );

			return true;
		}

		value = default;
		return false;
	}

    public void Clear()
    {
        while ( Count > 0 )
        {
            RemoveLast();
        }
    }

    public void SetCapacity( int newCapacity )
    {
        int delta = newCapacity - capacity;

        if ( delta > 0)
        {
            // Increase size
            for(int i = 0; i < delta; i++)
            {
                recycled.Enqueue( new LinkedListNode<KeyValuePair<Key, Value>>( default ) );
            }
        }
        else
        {
            // Decrease size
            delta *= -1;
            if ( Count > newCapacity )
            {
                // Remove extra cached items
                int toRemove = Count - newCapacity;
                for (int i = 0; i < toRemove; i++)
                {
                    RemoveLast();
                }
            }
            // Remove extra recycled nodes
            int targetRecycle = newCapacity - Count;
            while (recycled.Count > targetRecycle)
            {
                var node = recycled.Dequeue();
                node.Value = default; // Unassign anyways
            }
         }

        capacity = newCapacity;
    }

    public LinkedList<KeyValuePair<Key, Value>>.Enumerator GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator<KeyValuePair<Key, Value>> IEnumerable<KeyValuePair<Key, Value>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
