using System;
using System.Collections.Generic;

namespace Spatial
{
    /// <summary>
    /// A 2D grid of items.
    /// </summary>
    public class Grid<T>
    {
        public const float DefaultWorldSize = 8096f;
        public const int DefaultCellSize = 32; 
        
        /// <summary>
        /// How many cells wide and long
        /// </summary>
        public int CellCount { get; private set; }

        /// <summary>
        /// Each cells world size
        /// </summary>
        public int CellSize { get; private set; }

        private float CenterX;
        private float CenterY;

        private Node[,] Nodes;
        private Dictionary<T, Node> Lookup;

        public Grid( int CellSize = DefaultCellSize, float WorldSize = DefaultWorldSize)
        {
            this.CellSize = CellSize;
            this.CellCount = (int)((WorldSize / CellSize) + 0.5f);
            this.CenterX = WorldSize * 0.5f;
            this.CenterY = WorldSize * 0.5f;

            Nodes = new Node[CellCount, CellCount];
            Lookup = new Dictionary<T, Node>( 512 );
        }

        /// <summary>
        /// Please note that not all of these items will be inside this radius.
        /// This is a broadphase, so they will be near (in the same cell as the radius).
        /// If you need accuracy you should really post process these results to work out
        /// whether they actually are within.
        /// </summary>
        public int Query( float x, float y, float radius, T[] result, Func<T, bool> filter = null )
        {
            var minx = Clamp( (x + CenterX - radius) / CellSize );
            var maxx = Clamp( (x + CenterX + radius) / CellSize );
            var miny = Clamp( (y + CenterY - radius) / CellSize );
            var maxy = Clamp( (y + CenterY + radius) / CellSize );

            int found = 0;

            for ( int xx = minx; xx <= maxx; xx++ )
            {
                for ( int yy = miny; yy <= maxy; yy++ )
                {
                    if ( Nodes[xx, yy] == null ) continue;

                    foreach ( var t in Nodes[xx, yy].Contents )
                    {
                        if ( filter != null && filter( t ) == false )
                            continue;

                        result[found] = t;
                        found++;

                        if ( found >= result.Length )
                            return found;
                    }
                }
            }

            return found;
        }

        public void Query<U>( float x, float y, float radius, List<U> result) where U : class
        {
            if ( result == null )
                return;

            var minx = Clamp( (x + CenterX - radius) / CellSize );
            var maxx = Clamp( (x + CenterX + radius) / CellSize );
            var miny = Clamp( (y + CenterY - radius) / CellSize );
            var maxy = Clamp( (y + CenterY + radius) / CellSize );

            for ( int xx = minx; xx <= maxx; xx++ )
            {
                for ( int yy = miny; yy <= maxy; yy++ )
                {
                    if ( Nodes[xx, yy] == null ) continue;

                    foreach (T baseItem in Nodes[xx, yy].Contents)
                    {
                        if (baseItem is U derivedItem)
                            result.Add(derivedItem);
                    }
                }
            }
        }
        
        public void Query( float x, float y, float radius, List<T> result)
        {
            if ( result == null )
                return;

            var minx = Clamp( (x + CenterX - radius) / CellSize );
            var maxx = Clamp( (x + CenterX + radius) / CellSize );
            var miny = Clamp( (y + CenterY - radius) / CellSize );
            var maxy = Clamp( (y + CenterY + radius) / CellSize );

            for ( int xx = minx; xx <= maxx; xx++ )
                for ( int yy = miny; yy <= maxy; yy++ )
                    if ( Nodes[xx, yy] != null )
                        result.AddRange(Nodes[xx, yy].Contents);
        }
        
        public void Query( float x, float y, float radius, List<T> result, Func<T, bool> filter)
        {
            if ( result == null )
                return;

            var minx = Clamp( (x + CenterX - radius) / CellSize );
            var maxx = Clamp( (x + CenterX + radius) / CellSize );
            var miny = Clamp( (y + CenterY - radius) / CellSize );
            var maxy = Clamp( (y + CenterY + radius) / CellSize );

            for ( int xx = minx; xx <= maxx; xx++ )
            for ( int yy = miny; yy <= maxy; yy++ )
            {
                if (Nodes[xx, yy] == null) continue;
                    
                foreach ( var t in Nodes[xx, yy].Contents )
                {
                    if ( filter != null && !filter( t ) )
                        continue;

                    result.Add(t);
                }
            }
        }

        public bool Any(float x, float y, float radius, Func<T, bool> filter)
        {
            var minx = Clamp( (x + CenterX - radius) / CellSize );
            var maxx = Clamp( (x + CenterX + radius) / CellSize );
            var miny = Clamp( (y + CenterY - radius) / CellSize );
            var maxy = Clamp( (y + CenterY + radius) / CellSize );

            for ( int xx = minx; xx <= maxx; xx++ )
            for ( int yy = miny; yy <= maxy; yy++ )
            {
                if (Nodes[xx, yy] == null) continue;
                    
                foreach ( var t in Nodes[xx, yy].Contents )
                {
                    if ( filter != null && !filter( t ) )
                        continue;

                    return true;
                }
            }

            return false;
        }

        public void Subscribe(float x, float y, float radius, Action callback)
        {
            var minx = Clamp( (x + CenterX - radius) / CellSize );
            var maxx = Clamp( (x + CenterX + radius) / CellSize );
            var miny = Clamp( (y + CenterY - radius) / CellSize );
            var maxy = Clamp( (y + CenterY + radius) / CellSize );

            for ( int xx = minx; xx <= maxx; xx++ )
            for (int yy = miny; yy <= maxy; yy++)
            {
                var n = Nodes[xx, yy];
                if ( n == null )
                {
                    n = new Node();
                    Nodes[xx, yy] = n;
                }

                n.OnNodeContentsChanged += callback;
            }
        }

        internal class Node
        {
            public HashSet<T> Contents = new HashSet<T>();
            public event Action OnNodeContentsChanged;

            public void Add(T obj)
            {
                Contents.Add(obj);
                OnNodeContentsChanged?.Invoke();
            }

            public bool Remove(T obj)
            {
                bool r = Contents.Remove(obj);
                if(r) OnNodeContentsChanged?.Invoke();

                return r;
            }
        }

        int Clamp( float input )
        {
            var i = (int)input;

            if ( i < 0 ) return 0;
            if ( i > CellCount - 1 ) return CellCount - 1;

            return i;
        }

        Node GetNode( float x, float y, bool create = true )
        {
            x += CenterX;
            y += CenterY;

            int nodex = Clamp( x / CellSize );
            int nodey = Clamp( y / CellSize );

            var n = Nodes[nodex, nodey];
            if ( n == null && create )
            {
                n = new Node();
                Nodes[nodex, nodey] = n;
            }

            return n;
        }

        /// <summary>
        /// Add an object. No duplicate tests are done.
        /// </summary>
        public void Add( T obj, float x, float y )
        {
            var node = GetNode( x, y );
            node.Add( obj );
            Lookup.Add( obj, node );
        }

        /// <summary>
        /// Add an object. If the object already exists in this cell, it will not be added.
        /// </summary>
        /// <returns>True if the object was added, false if it already exists.</returns>
        public bool AddUnique(T obj, float x, float y)
        {
            if (Contains(obj))
                return false;

            Add( obj, x, y );
            return true;
        }

        public bool Contains( T obj ) => Lookup.ContainsKey( obj );

        public void Move( T obj, float x, float y )
        {
            var newNode = GetNode( x, y );
            if ( Lookup.TryGetValue( obj, out Node node ) )
            {
                if ( newNode == node )
                    return;

                node.Remove( obj );
                newNode.Add( obj );
                Lookup[obj] = newNode;
            }
        }

        public bool Remove( T obj )
        {
            Node node = null;
            if ( Lookup.TryGetValue( obj, out node ) )
            {
                node.Remove( obj );
                Lookup.Remove( obj );
                return true;
            }

            return false;
        }
    }
}
