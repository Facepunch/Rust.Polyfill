using System.Runtime.CompilerServices;

namespace Facepunch.Extend
{
    public static class ByteExtensions
    {
		/// <summary>
		/// In my tests this was 6x faster than doing BinaryReader.ReadT()
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T ReadUnsafe<T>( this byte[] buffer, int iOffset = 0 ) where T : unmanaged
		{
			fixed ( byte* ptr = buffer )
			{
				return *((T*)(ptr + iOffset));
			}
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void WriteUnsafe<T>( this byte[] buffer, in T value, int iOffset = 0 ) where T : unmanaged
		{
			fixed ( byte* ptr = buffer )
			{
				*((T*)(ptr + iOffset)) = value;
			}
		}
    }
}
