using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Facepunch.Extend
{
    public static class List
    {
        /// <summary>
        /// Compare two lists and categorize the difference into 3 lists - added, removed and remained
        /// Note: Assumes that a and b don't have duplicates!
        /// </summary>
        public static void Compare<T>( this List<T> a, List<T> b, List<T> added, List<T> removed, List<T> remained )
            where T : class
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
            if ( a == null && b == null ) return;

            // a is null, then everything is new
            if ( a == null )
            {
                added?.AddRange( b );
                return;
            }

            // b is null, then everything is removed
            if ( b == null )
            {
                removed?.AddRange( a );
                return;
            }

            if ( a.Count == 0 && b.Count == 0 ) return;

            HashSet<T> set = Pool.Get<HashSet<T>>();
            foreach (var objA in a)
            {
                set.Add(objA);
            }

            HashSet<T> alreadyProcessed = Pool.Get<HashSet<T>>();
            foreach (var objB in b)
            {
                if(alreadyProcessed.Contains(objB))
                {
                    continue;
                }

                if(set.Contains(objB))
                {
                    remained?.Add(objB);
                }
                else
                {
                    added?.Add(objB);
                }
                alreadyProcessed.Add(objB);
            }

            set.Clear();
            foreach (var objB in b)
            {
                set.Add(objB);
            }

            foreach (var objA in a)
            {
                if (alreadyProcessed.Contains(objA))
                {
                    continue;
                }

                if (set.Contains(objA))
                {
                    remained?.Add(objA);
                }
                else
                {
                    removed?.Add(objA);
                }
                alreadyProcessed.Add(objA);
            }

            Pool.FreeUnmanaged(ref alreadyProcessed);
            Pool.FreeUnmanaged(ref set);
        }

        public static void Compare<TListA, TListB, TItemA, TItemB, TKey>(
            this TListA a, TListB b, HashSet<TKey> added, HashSet<TKey> removed, HashSet<TKey> remained, Func<TItemA, TKey> selectorA, Func<TItemB, TKey> selectorB )
            where TListA : IEnumerable<TItemA>
            where TListB : IEnumerable<TItemB>
            where TKey : IEquatable<TKey>
        {
            if ( a == null ) throw new ArgumentNullException( nameof( a ) );
            if ( b == null ) throw new ArgumentNullException( nameof( b ) );
            if ( added == null ) throw new ArgumentNullException( nameof( added ) );
            if ( removed == null ) throw new ArgumentNullException( nameof( removed ) );
            if ( remained == null ) throw new ArgumentNullException( nameof( remained ) );

            // which ones do we have already?           bKeys.IntersectWith(aKeys)
            // which ones are new?                      bKeys.ExceptWith(aKeys)
            // which ones do we no longer need?         aKeys.ExceptWith(bKeys)
            // To do this in place we start with projecting all the values to keys:
            //   added = bKeys (aka selectorB(x) for x in b)
            //   removed = aKeys (aka selectorA(x) for x in a)
            //
            // Then we just extract the common part from either keyset:
            //   remained = added IntersectsWith removed
            // And we remove the common part from either set:
            //   added.ExceptWith(remained)
            //   removed.ExceptWith(remained)
            added.Clear();
            foreach (var item in b) added.Add(selectorB(item));

            removed.Clear();
            foreach (var item in a) removed.Add(selectorA(item));

            remained.Clear();
            foreach(var key in removed)
            {
                if(added.Contains(key))
                {
                    remained.Add(key);
                }
            }
            added.ExceptWith(remained);
            removed.ExceptWith(remained);
        }

        public static TItem FindWith<TItem, TKey>( this IReadOnlyCollection<TItem> items, Func<TItem, TKey> selector, TKey search, IEqualityComparer<TKey> comparer = null )
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;

            foreach ( var item in items )
            {
                if ( comparer.Equals( selector( item ), search ) )
                    return item;
            }

            return default(TItem);
        }

        public static TItem? TryFindWith<TItem, TKey>( this IReadOnlyCollection<TItem> items, Func<TItem, TKey> selector, TKey search, IEqualityComparer<TKey> comparer = null )
            where TItem : struct
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;

            foreach ( var item in items )
            {
                if ( comparer.Equals( selector( item ), search ) )
                    return item;
            }

            return null;
        }

        public static int FindIndexWith<TItem, TKey>( this IReadOnlyList<TItem> items, Func<TItem, TKey> selector, TKey search, IEqualityComparer<TKey> comparer = null )
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;

            for ( var i = 0; i < items.Count; i++ )
            {
                var item = items[i];
                if ( comparer.Equals( search, selector( item ) ) )
                    return i;
            }

            return -1;
        }

        public static int FindIndex<TItem>( this IReadOnlyList<TItem> items, TItem search, IEqualityComparer<TItem> comparer = null )
        {
            comparer = comparer ?? EqualityComparer<TItem>.Default;

            for ( var i = 0; i < items.Count; i++ )
            {
                var item = items[i];
                if ( comparer.Equals( search, item ) )
                    return i;
            }

            return -1;
        }

        public static List<T> ShallowClonePooled<T>( this List<T> items )
        {
            if ( items == null )
            {
                return null;
            }

            var clone = Pool.Get<List<T>>();
            foreach ( var item in items )
            {
                clone.Add( item );
            }
            return clone;
        }

        public static bool Resize<T>(this List<T> list, int newCount)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (newCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newCount));
            }

            if (list.Count == newCount)
            {
                return false;
            }

            if (list.Count > newCount)
            {
                while (list.Count > newCount)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
            else
            {
                while (list.Count < newCount)
                {
                    list.Add(default);
                }
            }

            return true;
        }
    }
}
