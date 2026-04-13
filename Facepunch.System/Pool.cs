using System;
using System.Globalization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

//
// This is kept very simple by design.
// This should be kept platform agnostic at all costs
//

namespace Facepunch
{
#if UNITY_EDITOR
    internal static class MemTrackerHelpers
    {
        public static Func<T, long> CreateMemoryEstimator<T>() where T : class
        {
            // There are a couple collections that retain it's capacity
            // when we return it to the pool. It adds memory overhead - try
            // to estimate it
            Type type = typeof(T);
            Type genericTypeDef = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            if (genericTypeDef == typeof(List<>)
                || genericTypeDef == typeof(BufferList<>))
            {
                int elemSize = SizeOf(type.GenericTypeArguments[0]);
                PropertyInfo property = type.GetProperty("Capacity",
                    BindingFlags.Instance | BindingFlags.Public);
                return (T obj) => {
                    // property.GetValue(obj) causes boxing, despite the fact
                    // that T is a ref type already. Only way to "fix" this is
                    // to generate our own IL - I'll do that separately
                    object propValue = property.GetValue(obj);
                    return Convert.ToInt64(propValue) * elemSize;
                };
            }
            else if (type == typeof(System.IO.MemoryStream)
                || type == typeof(StringBuilder))
            {
                PropertyInfo property = type.GetProperty("Capacity",
                    BindingFlags.Instance | BindingFlags.Public);
                return (T obj) => {
                    object propValue = property.GetValue(obj);
                    return Convert.ToInt64(propValue);
                };
            }
            return null;
        }

        private static int SizeOf(Type type)
        {
            if (type.IsClass || type.IsInterface)
            {
                // legally speaking, this is invalid for interface cases,
                // but pooling interfaces is already dubious and will
                // cause boxing - so this works as an approximation
                return IntPtr.Size;
            }
            else if (type.IsValueType)
            {
                return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf(type);
            }
            // uh oh
            return 1;
        }
    }
#endif


    public static class Pool
    {
#if UNITY_EDITOR
        // Disabled by default as it generates GC allocs due to boxing
        public static bool ExtraMemTrackingEnabled = false;
#endif

        public interface IPooled
        {
            void EnterPool();
            void LeavePool();
        }

        public interface IPoolCollection
        {
            // Using long instead of ulong to allow comparison operators with integers
            long ItemsCapacity { get; }
            long ItemsInStack { get; }
            long ItemsInUse { get; }
            long ItemsCreated { get; }
            long ItemsTaken { get; }
            long ItemsSpilled { get; }
            long MaxItemsInUse { get; }

#if UNITY_EDITOR
            long ExtraMem { get; }
#endif

            void Reset();
            void ResetMaxUsageCounter();
            void Add(object obj);
        }

        public class PoolCollection<T> : IPoolCollection where T : class, new()
        {
            private static readonly object collectionLock = new object();

            private BufferList<T> buffer;

#if UNITY_EDITOR
            private HashSet<T> hashset;
#endif

            public long ItemsCapacity { get; private set; }
            public long ItemsInStack { get; private set; }
            public long ItemsInUse { get; private set; }
            public long ItemsCreated { get; private set; }
            public long ItemsTaken { get; private set; }
            public long ItemsSpilled { get; private set; }
            public long MaxItemsInUse { get; private set; }
#if UNITY_EDITOR
            public long ExtraMem { get; private set; }

            public Func<T, long> EstimateMem;
#endif

            public PoolCollection()
            {
                Resize( 512 );
            }

            public void Reset()
            {
                Resize( (int)ItemsCapacity );
            }

            public void ResetMaxUsageCounter()
            {
                lock (collectionLock)
                {
                    MaxItemsInUse = ItemsInUse;
                }
            }

            public void Resize( int size )
            {
                lock ( collectionLock )
                {
#if UNITY_EDITOR
                    hashset = new HashSet<T>( size );
                    ExtraMem = 0;
#endif

                    buffer = new BufferList<T>( size );

                    ItemsCapacity = size;
                    ItemsInStack = 0;
                    ItemsInUse = 0;
                    ItemsCreated = 0;
                    ItemsTaken = 0;
                    ItemsSpilled = 0;
                    MaxItemsInUse = 0;
                }
            }

            public void Add( T obj )
            {
                //
                // Notify the interface, if available
                // NOTE: Notify even if the pool is at capacity so any cleanup can still be performed
                //
                var poolable = obj as IPooled;
                if ( poolable != null )
                {
                    poolable.EnterPool();
                }

                lock ( collectionLock )
                {
                    ItemsInUse--;

                    if ( ItemsInStack < ItemsCapacity )
                    {
#if UNITY_EDITOR
                        bool added = hashset.Add( obj );
                        if(!added)
                        {
                            throw new SystemException("PoolCollection.Add called with item that is already in the pool");
                        }
                        ExtraMem += EstimateMem?.Invoke(obj) ?? 0;
#endif

                        buffer.Push( obj );

                        ItemsInStack++;
                    }
                    else
                    {
                        ItemsSpilled++;
                    }
                }
            }

            public T Take()
            {
                T obj;

                lock ( collectionLock )
                {
                    ItemsInUse++;
                    MaxItemsInUse = Math.Max( ItemsInUse, MaxItemsInUse );

                    if ( ItemsInStack > 0 )
                    {
                        obj = buffer.Pop();

#if UNITY_EDITOR
                        if (obj == null)
                        {
                            throw new SystemException("PoolCollection.Take retrieved null item");
                        }

                        hashset.Remove( obj );
                        ExtraMem -= EstimateMem?.Invoke(obj) ?? 0;
#endif

                        ItemsInStack--;
                        ItemsTaken++;
                    }
                    else
                    {
                        obj = new T();

                        ItemsCreated++;
                    }
                }

                //
                // Notify the interface, if available
                //
                var poolable = obj as IPooled;
                if ( poolable != null )
                {
                    poolable.LeavePool();
                }

                return obj;
            }

            public void Fill()
            {
                var count = ItemsCapacity - ItemsInStack;

                for ( int i = 0; i < count; i++ )
                {
                    var obj = new T();

                    var poolable = obj as IPooled;
                    if ( poolable != null )
                    {
                        poolable.EnterPool();
                    }

                    lock ( collectionLock )
                    {
#if UNITY_EDITOR
                        hashset.Add( obj );
                        ExtraMem += EstimateMem?.Invoke(obj) ?? 0;
#endif

                        buffer.Push( obj );

                        ItemsInStack++;
                    }
                }
            }

            void IPoolCollection.Add( object obj )
            {
                Add( (T)obj );
            }
        }

        public static ConcurrentDictionary<System.Type, IPoolCollection> Directory = new ConcurrentDictionary<System.Type, IPoolCollection>();

        /// <summary>
        /// Return the <paramref name="obj"/> to the <see cref="Pool"/> and calls <see cref="IPooled.EnterPool"/> to reset it's state.
        /// <br/> If <typeparamref name="T"/> isn't <see cref="IPooled"/> then use <see cref="FreeUnmanaged{T}(ref T)"/>
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Pool.Get{T}"/></param>
        public static void Free<T>( ref T obj ) where T : class, IPooled, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            FreeInternal( ref obj );
        }

        /// <summary>
        /// <inheritdoc cref="Free{T}(ref T)"/> <br/><br/> 
        /// Calls <see cref="List{T}.Clear()"/> <br/>
        /// If <paramref name="freeElements"/> is <see langword="true"/>, calls <see cref="Free{T}(ref T)"/> on elements.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        /// <param name="freeElements">Should each element be returned to Pool via <see cref="Free{T}(ref T)"/>. By default it's <see langword="false"/></param>
        public static void Free<T>(ref List<T> obj, bool freeElements = false) where T : class, IPooled, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (freeElements)
            {
                foreach (var item in obj)
                {
                    if (item != null)
                    {
                        var copy = item;
                        Free(ref copy);
                    }
                }
            }
            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// <inheritdoc cref="Free{T}(ref T)"/> <br/><br/> 
        /// Calls <see cref="HashSet{T}.Clear()"/> <br/>
        /// If <paramref name="freeElements"/> is <see langword="true"/>, calls <see cref="Free{T}(ref T)"/> on elements.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        /// <param name="freeElements">Should each element be returned to Pool via <see cref="Free{T}(ref T)"/>. By default it's <see langword="false"/></param>
        public static void Free<T>(ref HashSet<T> obj, bool freeElements = false) where T : class, IPooled, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (freeElements)
            {
                foreach (var item in obj)
                {
                    if (item != null)
                    {
                        var copy = item;
                        Free(ref copy);
                    }
                }
            }
            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// <inheritdoc cref="Free{T}(ref T)"/> <br/><br/> 
        /// Calls <see cref="Dictionary{TKey, TVal}.Clear()"/> <br/>
        /// If <paramref name="freeElements"/> is <see langword="true"/>, calls <see cref="Free{T}(ref T)"/> on elements.
        /// </summary>
        /// <param name="dict">Non-null object previously allocated via <see cref="Get{T}"/></param>
        /// <param name="freeElements">Should each Value be returned to Pool via <see cref="Free{T}(ref T)"/>. By default it's <see langword="false"/></param>
        public static void Free<TKey, TVal>(ref Dictionary<TKey, TVal> dict, bool freeElements = false) 
            where TVal : class, IPooled, new()
        {
            if (dict == null)
            {
                throw new ArgumentNullException();
            }

            if (freeElements)
            {
                foreach (var kvp in dict)
                {
                    if (kvp.Value != null)
                    {
                        var copy = kvp.Value;
                        Free(ref copy);
                    }
                }
            }
            dict.Clear();
            FreeInternal(ref dict);
        }

        /// <summary>
        /// <inheritdoc cref="Free{T}(ref T)"/> <br/><br/> 
        /// Calls <see cref="BufferList{T}.Clear()"/> <br/>
        /// If <paramref name="freeElements"/> is <see langword="true"/>, calls <see cref="Free{T}(ref T)"/> on elements.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        /// <param name="freeElements">Should each element be returned to Pool via <see cref="Free{T}(ref T)"/>. By default it's <see langword="false"/></param>
        public static void Free<T>(ref BufferList<T> obj, bool freeElements = false) where T : class, IPooled, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (freeElements)
            {
                foreach (var item in obj)
                {
                    if (item != null)
                    {
                        var copy = item;
                        Free(ref copy);
                    }
                }
            }
            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// <inheritdoc cref="Free{T}(ref T)"/> <br/><br/> 
        /// Calls <see cref="ListDictionary{TKey, TVal}.Clear()"/> <br/>
        /// If <paramref name="freeElements"/> is <see langword="true"/>, calls <see cref="Free{T}(ref T)"/> on elements.
        /// </summary>
        /// <param name="dict">Non-null object previously allocated via <see cref="Get{T}"/></param>
        /// <param name="freeElements">Should each Value be returned to Pool via <see cref="Free{T}(ref T)"/>. By default it's <see langword="false"/></param>
        public static void Free<TKey, TVal>(ref ListDictionary<TKey, TVal> dict, bool freeElements = false)
            where TVal : class, IPooled, new()
        {
            if (dict == null)
            {
                throw new ArgumentNullException();
            }

            if (freeElements)
            {
                for(int i=0; i<dict.Values.Count; i++)
                {
                    var copy = dict.Values[i];
                    if (copy != null)
                    {
                        Free(ref copy);
                    }
                }
            }
            dict.Clear();
            FreeInternal(ref dict);
        }

        /// <summary>
        /// <inheritdoc cref="Free{T}(ref T)"/> <br/><br/> 
        /// Calls <see cref="Queue{T}.Clear()"/> <br/>
        /// If <paramref name="freeElements"/> is <see langword="true"/>, calls <see cref="Free{T}(ref T)"/> on elements.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        /// <param name="freeElements">Should each element be returned to Pool via <see cref="Free{T}(ref T)"/>. By default it's <see langword="false"/></param>
        public static void Free<T>(ref Queue<T> obj, bool freeElements = false) where T : class, IPooled, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (freeElements)
            {
                foreach (var item in obj)
                {
                    if (item != null)
                    {
                        var copy = item;
                        Free(ref copy);
                    }
                }
            }
            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// <inheritdoc cref="Free{T}(ref T)"/> <br/><br/> 
        /// Calls <see cref="ListHashSet{T}.Clear()"/> <br/>
        /// If <paramref name="freeElements"/> is <see langword="true"/>, calls <see cref="Free{T}(ref T)"/> on elements.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        /// <param name="freeElements">Should each element be returned to Pool via <see cref="Free{T}(ref T)"/>. By default it's <see langword="false"/></param>
        public static void Free<T>(ref ListHashSet<T> obj, bool freeElements = false) where T : class, IPooled, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            if (freeElements)
            {
                foreach (var item in obj)
                {
                    if (item != null)
                    {
                        var copy = item;
                        Free(ref copy);
                    }
                }
            }
            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool without resetting it's state.
        /// <br/><br/> 
        /// Avoid using this overload if possible. Instead, add implement <see cref="IPooled"/> and then use <see cref="Free{T}(ref T)"/>
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnsafe<T>(ref T obj) where T : class, new()
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="System.IO.MemoryStream.SetLength(long)"/> method.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged(ref System.IO.MemoryStream obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.SetLength(0);
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="StringBuilder.Clear()"/> method.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged(ref StringBuilder obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="System.Diagnostics.Stopwatch.Reset()"/> method.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged(ref System.Diagnostics.Stopwatch obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.Reset();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="List{T}.Clear()"/> method. <br/><br/>
        /// Prefer to use <see cref="Free{T}(ref List{T}, bool)"/> if possible.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged<T>(ref List<T> obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="HashSet{T}.Clear()"/> method. <br/><br/>
        /// Prefer to use <see cref="Free{T}(ref HashSet{T}, bool)"/> if possible.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged<T>(ref HashSet<T> obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="Dictionary{TKey, TVal}.Clear()"/> method. <br/><br/>
        /// Prefer to use <see cref="Free{T}(ref Dictionary{TKey, TVal}, bool)"/> if possible.
        /// </summary>
        /// <param name="dict">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged<TKey, TVal>(ref Dictionary<TKey, TVal> dict) 
        {
            if (dict == null)
            {
                throw new ArgumentNullException();
            }

            dict.Clear();
            FreeInternal(ref dict);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="BufferList{T}.Clear()"/> method. <br/><br/>
        /// Prefer to use <see cref="Free{T}(ref BufferList{T}, bool)"/> if possible.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged<T>(ref BufferList<T> obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="ListDictionary{TKey, TVal}.Clear()"/> method. <br/><br/>
        /// Prefer to use <see cref="Free{T}(ref ListDictionary{TKey, TVal}, bool)"/> if possible.
        /// </summary>
        /// <param name="dict">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged<TKey, TVal>(ref ListDictionary<TKey, TVal> dict) 
        {
            if (dict == null)
            {
                throw new ArgumentNullException();
            }

            dict.Clear();
            FreeInternal(ref dict);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="Queue{T}.Clear()"/> method. <br/><br/>
        /// Prefer to use <see cref="Free{T}(ref Queue{T}, bool)"/> if possible.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged<T>(ref Queue<T> obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.Clear();
            FreeInternal(ref obj);
        }

        /// <summary>
        /// Return the <paramref name="obj"/> to the Pool and reset it's state via <see cref="ListHashSet{T}.Clear()"/> method. <br/><br/>
        /// Prefer to use <see cref="Free{T}(ref ListHashSet{T}, bool)"/> if possible.
        /// </summary>
        /// <param name="obj">Non-null object previously allocated via <see cref="Get{T}"/></param>
        public static void FreeUnmanaged<T>(ref ListHashSet<T> obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            obj.Clear();
            FreeInternal(ref obj);
        }

        private static void FreeInternal<T>( ref T obj ) where T : class, new()
        {
            var collection = FindCollection<T>();

            collection.Add( obj );

            //
            // Reset the pointer so it can't be used anymore
            //
            obj = null;
        }

        /// <summary>
        /// A pooled version of calling "new T()"
        /// </summary>
        public static T Get<T>() where T : class, new()
        {
            var collection = FindCollection<T>();

            return collection.Take();
        }

        /// <summary>
        /// Resizes the pool buffer for a type.
        /// </summary>
        public static void ResizeBuffer<T>( int size ) where T : class, new()
        {
            var collection = FindCollection<T>();

            collection.Resize( size );
        }

        /// <summary>
        /// Fills the pool buffer for a type.
        /// </summary>
        public static void FillBuffer<T>() where T : class, new()
        {
            var collection = FindCollection<T>();
            
            collection.Fill();
        }

        /// <summary>
        /// Gets a PoolCollection. Use for diagnostics, debug printing.
        /// </summary>
        public static PoolCollection<T> FindCollection<T>() where T : class, new()
        {
            return Pool<T>.Collection;
        }

        public static void Clear( string filter = null )
        {
            if ( string.IsNullOrEmpty( filter ) )
            {
                foreach (var c in Directory)
                {
                    c.Value.Reset();
                }
            }
            else
            {
                foreach (var c in Directory)
                {
                    var name = c.Key.FullName;

                    if ( name.Contains( filter, CompareOptions.IgnoreCase ) )
                    {
                        c.Value.Reset();
                    }
                }
            }
        }

        private static bool Contains( this string haystack, string needle, CompareOptions options )
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf( haystack, needle, options ) >= 0;
        }
    }

    internal static class Pool<T> where T : class, new()
    {
        public static Pool.PoolCollection<T> Collection;

        static Pool()
        {
            Collection = new Pool.PoolCollection<T>()
            {
#if UNITY_EDITOR
                EstimateMem = Pool.ExtraMemTrackingEnabled ? MemTrackerHelpers.CreateMemoryEstimator<T>() : null
#endif
            };
            Pool.Directory[ typeof( T ) ] = Collection;
        }
    }
}
