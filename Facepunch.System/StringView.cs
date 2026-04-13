using System;
using System.Collections.Generic;
using System.Reflection;

namespace Facepunch
{
    /// <summary>
    /// <see cref="StringView"/> is an owning, immutable view into a string that can be
    /// explicitly converted back to a string.
    /// <list type="bullet">
    /// <item>Unlike <see cref="ReadOnlySpan{T}"/>,
    /// <see cref="StringView"/> can participate in more use-cases (class member variables,
    /// dictionary keys, etc).</item>
    /// <item>Unlike <see cref="System.Memory{T}"/> being a generic memory span,
    /// <see cref="StringView"/> emulates <see cref="String"/> API as much as possible and allows
    /// <see cref="StringView"/>s to be equal even if they're formed from different strings.</item>
    /// </list>
    /// Use it as a tool to reduce string-related allocations
    /// in read-focused scenarios, as those are allocation free where possible.
    /// <br/><br/>
    /// Be aware - conversion back to string requires an allocation.
    /// </summary>
    public readonly struct StringView
    {
        readonly string _source;
        readonly int _start; // inclusive
        readonly int _end; // exclusive

        public int Length => _end - _start;

        public char this[int index]
        {
            get
            {
                if ( index >= Length || index < 0 )
                {
                    throw new ArgumentOutOfRangeException( "index" );
                }
                return _source[_start + index];
            }
        }

        /// <summary>
        /// Constructs a <see cref="StringView"/> over the entire <paramref name="source"/>
        /// </summary>
        /// <param name="source">A <see cref="string"/> into which a view is created. Will be retained!</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> was null!</exception>
        public StringView( string source )
        {
            if ( source == null )
            {
                throw new ArgumentNullException( "source" );
            }

            _source = source;
            _start = 0;
            _end = _source.Length;
        }

        /// <summary>
        /// Constructs a <see cref="StringView"/> over the entire <paramref name="source"/> starting at <paramref name="start"/>
        /// </summary>
        /// <param name="source">A <see cref="string"/> into which a view is created. Will be retained!</param>
        /// <param name="start">Offset into <paramref name="source"/> for start</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> was null!</exception>
        /// <exception cref="ArgumentOutOfRangeException">Formed invalid view, check <paramref name="start"/></exception>
        public StringView( string source, int start )
        {
            if ( source == null )
            {
                throw new ArgumentNullException( "source" );
            }

            _source = source;
            _start = start;
            _end = _source.Length;
            if ( _start > _end )
            {
                throw new ArgumentOutOfRangeException( $"Invalid view arguments: start({_start}) is after end({_end})!" );
            }
            if ( _start < 0 )
            {
                throw new ArgumentOutOfRangeException( $"Start({_start}) was past the start of string!" );
            }
        }

        /// <summary>
        /// Constructs a <see cref="StringView"/> into <paramref name="source"/> with a
        /// view of [<paramref name="start"/>, <paramref name="start"/> + <paramref name="length"/>)
        /// </summary>
        /// <param name="source">A <see cref="string"/> into which a view is created. Will be retained!</param>
        /// <param name="start">Offset into <paramref name="source"/> for start</param>
        /// <param name="length">Length of view into <paramref name="source"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> was null!</exception>
        /// <exception cref="ArgumentOutOfRangeException">Formed invalid view, check <paramref name="start"/>, <paramref name="length"/></exception>
        public StringView( string source, int start, int length )
        {
            if ( source == null )
            {
                throw new ArgumentNullException( "source" );
            }

            _source = source;
            _start = start;
            _end = _start + length;
            if ( _start > _end )
            {
                throw new ArgumentOutOfRangeException( $"Invalid view arguments: start({_start}) is after end({_end})!" );
            }
            if ( _end > _source.Length )
            {
                throw new ArgumentOutOfRangeException( $"End({_end}) was past the length of string!" );
            }
            if ( _start < 0 )
            {
                throw new ArgumentOutOfRangeException( $"Start({_start}) was past the start of string!" );
            }
        }

        public static implicit operator StringView( string source ) => new StringView( source );
        public static implicit operator ReadOnlySpan<char>( StringView view )
        {
            return view._source.AsSpan( view._start, view.Length );
        }

        /// <inheritdoc cref="StringView.ToString"/>
        /// <param name="view">View to convert back to <see cref="string"/></param>
        public static explicit operator string( StringView view )
        {
            return view.ToString();
        }

        /// <summary>
        /// Either returns the underlying <see cref="_source"/> if view covers it's entirety, or allocates a new <see cref="string"/>
        /// </summary>
        public override string ToString()
        {
            return _source.Substring( _start, Length );
        }

        /// <summary>
        /// Calculates hash for <see cref="StringView"/> using the same logic as <see cref="string"/> and returns it.
        /// <code>StringView("Hello world!").GetHashCode() == "Hello world!".GetHashCode() => true</code>
        /// </summary>
        public override int GetHashCode()
        {
            // using same hash implementation as Unity, so that we can treat StringView as a String
            // source: https://github.com/Unity-Technologies/mono/blob/unity-main/external/corefx-bugfix/src/Common/src/CoreLib/System/String.Comparison.cs#L871
            // Note: discarded 32bit impl as we ship for 64bit arch
            int hash1 = 5381;
            int hash2 = hash1;
            for ( int i = _start; i != _end; )
            {
                int c = _source[i++];
                hash1 = ( ( hash1 << 5 ) + hash1 ) ^ c;
                if ( i == _end )
                    break;
                c = _source[i++];
                hash2 = ( ( hash2 << 5 ) + hash2 ) ^ c;
            }
            return hash1 + ( hash2 * 1566083941 );
        }

        public override bool Equals( object obj ) => Equals( obj, StringComparison.CurrentCulture );

        // Maybe want to update the Equals(StringView) method to support StringComparison, but that adds a lot more complexity to that function.
        public bool Equals( object obj, StringComparison comparisonOptions )
        {
            if ( obj is StringView otherSv )
            {
                return Equals( otherSv );
            }

            if ( obj is string otherStr )
            {
                return otherStr.Length == Length && string.Compare( _source, _start, otherStr, 0, Length, comparisonOptions ) == 0;
            }

            throw new ArgumentException( $"Unsupported type for equality check! Other object was {obj.GetType()}" );
        }
        
        /// Compares this string view to a concrete String
        public bool Equals(string other)
        {
            if (other == null || other.Length != Length)
                return false;

            for (int i = 0; i < Length; i++)
            {
                if (_source[_start + i] != other[i])
                    return false;
            }

            return true;
        }
        
        /// Compares this string view to a concrete char
        public bool Equals(char other)
        {
            return Length == 1 && _source[_start] == other;
        }

        public bool Equals( StringView otherSv )
        {
            if ( Length != otherSv.Length )
            {
                return false;
            }

            for ( int i = 0; i != Length; i++ )
            {
                if ( _source[_start + i] != otherSv._source[otherSv._start + i] )
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==( StringView left, StringView right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( StringView left, StringView right )
        {
            return !left.Equals( right );
        }

        /// <summary>
        /// Splits <see cref="StringView"/> by <paramref name="delim"/> into chunks and pushes them to <paramref name="collection"/>.
        /// <br/>
        /// Behaves like: <code>"Hello world!".Split(' ', StringSplitOptions.RemoveEmptyEntries) => ["Hello", "world!"]</code>
        /// </summary>
        /// <param name="delim">Single char delimeter to use</param>
        /// <param name="collection">Buffer to accumulate reults to. Doesn't not clear it!</param>
        public void Split( char delim, ICollection<StringView> collection )
        {
            int lastOffset = _start;
            for ( int i = _start; i != _end; i++ )
            {
                if ( _source[i] == delim )
                {
                    if ( lastOffset != i ) // avoids delimdelim scenario
                    {
                        collection.Add( new StringView( _source, lastOffset, i - lastOffset ) );
                    }
                    lastOffset = i + 1;
                }
            }
            if ( lastOffset != _end )
            {
                collection.Add( new StringView( _source, lastOffset, _end - lastOffset ) );
            }
        }

        /// <summary>
        /// Checks if <see cref="StringView"/> starts with <paramref name="other"/>. Case-sensitive.
        /// <br/>
        /// Behaves like: <code>"Hello world!".StartsWith("Hello") => true</code>
        /// </summary>
        /// <param name="other">Substring to check against</param>
        /// <returns><see langword="true"/> if <see cref="StringView"/> starts with <paramref name="other"/>, otherwise <see langword="false"/></returns>
        public bool StartsWith( StringView other )
        {
            if ( other.Length > Length )
            {
                return false;
            }

            for ( int i = 0; i < other.Length; i++ )
            {
                if ( _source[_start + i] != other._source[other._start + i] )
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a sub-<see cref="StringView"/> starting at <paramref name="offset"/>
        /// Equivalent to: <code>"Hello world!".Substring(6) => "world!"</code>
        /// </summary>
        /// <param name="offset">An offset from the start of <see cref="StringView"/></param>
        /// <returns>A sub-<see cref="StringView"/> with an <paramref name="offset"/></returns>
        public StringView Substring( int offset )
        {
            if ( offset > Length || offset < 0 )
            {
                throw new ArgumentOutOfRangeException( "offset" );
            }
            return new StringView( _source, _start + offset, Length - offset );
        }

        /// <summary>
        /// Returns a sub-<see cref="StringView"/> starting at <paramref name="offset"/> with specicif <paramref name="length"/>
        /// Equivalent to: <code>"Hello world!".Substring(6, 5) => "world"</code>
        /// </summary>
        /// <param name="offset">An offset from the start of <see cref="StringView"/></param>
        /// <param name="length">Length of the new <see cref="StringView"/>, must be contained within!</param>
        /// <returns>A sub-<see cref="StringView"/> with an <paramref name="offset"/> and <paramref name="length"/></returns>
        public StringView Substring( int offset, int length )
        {
            if ( offset > Length || offset < 0 )
            {
                throw new ArgumentOutOfRangeException( "offset" );
            }
            if ( _start + offset + length > _end ) // ctor checks length<0, so we skip here
            {
                throw new ArgumentOutOfRangeException( "length" );
            }
            return new StringView( _source, _start + offset, length );
        }

        /// <summary>
        /// Impicit Range support
        /// <br/><br/>
        /// Returns a sub-<see cref="StringView"/> starting at <paramref name="offset"/> with specicif <paramref name="length"/>
        /// Equivalent to: <code>"Hello world!".Substring(6, 5) => "world"</code>
        /// </summary>
        /// <param name="offset">Start of slice, must be positive and within <see cref="StringView"/>!</param>
        /// <param name="length">Length of slice, must be positive and within <see cref="StringView"/></param>
        /// <returns>A sub-<see cref="StringView"/> with an <paramref name="offset"/> and <paramref name="length"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">The slice must be fully contained within <see cref="StringView"/></exception>
        public StringView Slice( int offset, int length )
        {
            return Substring( offset, length );
        }

        /// <summary>
        /// Checks whether <paramref name="other"/> is present in this <see cref="StringView"/>
        /// Equivalent to: <code>"Hello world!".Contains("world") => true</code>
        /// </summary>
        /// <param name="other">A <see cref="StringView"/> to search for</param>
        /// <returns><see langword="true"/> if <see cref="StringView"/> starts with <paramref name="other"/>, otherwise <see langword="false"/></returns>
        public bool Contains( StringView other )
        {
            if ( other.Length > Length )
            {
                return false;
            }

            int foundMatching = 0;
            for ( int i = 0; i < Length; i++ )
            {
                if ( _source[_start + i] == other._source[other._start + foundMatching] )
                {
                    foundMatching++;
                    if ( foundMatching == other._end - other._start )
                    {
                        return true;
                    }
                }
                else
                {
                    foundMatching = 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the index of <paramref name="other"/> in this <see cref="StringView"/> if there is one.
        /// Equivalent to: <code>"Hello world!".IndexOf("world") => 6</code>
        /// </summary>
        /// <param name="other">A <see cref="StringView"/> to search for</param>
        /// <returns>Zero-based index where <paramref name="other"/> starts in this <see cref="StringView"/>, or -1 otherwise</returns>
        public int IndexOf( StringView other )
        {
            if ( other.Length > Length )
            {
                return -1;
            }

            int foundMatching = 0;
            for ( int i = 0; i < Length; i++ )
            {
                if ( _source[_start + i] == other._source[other._start + foundMatching] )
                {
                    foundMatching++;
                    if ( foundMatching == other._end - other._start )
                    {
                        return i - foundMatching + 1;
                    }
                }
                else
                {
                    foundMatching = 0;
                }
            }
            return -1;
        }

        /// <summary>
        /// Comparator to support <see cref="StringComparer.OrdinalIgnoreCase"/> scenario.
        /// Instead of creating new ones, prefer using <see cref="ComparerIgnoreCase.Instance"/>
        /// </summary>
        public class ComparerIgnoreCase : EqualityComparer<StringView>
        {
            public static ComparerIgnoreCase Instance = new ComparerIgnoreCase();

            public override bool Equals( StringView x, StringView y )
            {
                if ( x.Length != y.Length )
                {
                    return false;
                }

                for ( int i = 0; i != x.Length; i++ )
                {
                    char a = x._source[x._start + i];
                    char b = y._source[y._start + i];

                    bool isSameLetterIgnoreCase = ( a == b ) || ( ( a | 0x20 ) == ( b | 0x20 )
                        && (uint)( ( a | 0x20 ) - 'a' ) <= (uint)( 'z' - 'a' ) );
                    if ( !isSameLetterIgnoreCase )
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode( StringView obj )
            {
                // Same as StringView.GetHashCode(), only it lowercases all to get the hash
                int hash1 = 5381;
                int hash2 = hash1;
                for ( int i = obj._start; i != obj._end; )
                {
                    int c = obj._source[i++];
                    if ( (uint)( ( c | 0x20 ) - 'a' ) <= (uint)( 'z' - 'a' ) )
                    {
                        c |= 0x20;
                    }
                    hash1 = ( ( hash1 << 5 ) + hash1 ) ^ c;
                    if ( i == obj._end )
                        break;

                    c = obj._source[i++];
                    if ( (uint)( ( c | 0x20 ) - 'a' ) <= (uint)( 'z' - 'a' ) )
                    {
                        c |= 0x20;
                    }
                    hash2 = ( ( hash2 << 5 ) + hash2 ) ^ c;
                }
                return hash1 + ( hash2 * 1566083941 );
            }
        }
    }
}
