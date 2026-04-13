#region ProtocolParser
using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Facepunch.Extend;
using UnityEngine;
using UnityEngine.Profiling;
using SilentOrbit.ProtocolBuffers;

namespace SilentOrbit.ProtocolBuffers
{
	public interface IProto
	{
		void WriteToStream( BufferStream stream );
		void ReadFromStream( BufferStream stream, bool isDelta = false );
		void ReadFromStream( BufferStream stream, int size, bool isDelta = false );
	}

	public interface IProto<in T> : IProto
		where T : IProto
	{
		void WriteToStreamDelta( BufferStream stream, T previousProto );
		
		void CopyTo( T other );
	}
	
	public static partial class ProtocolParser
	{
		private const int staticBufferSize = 128 * 1024;
		
		// Seperate copy of buffer per thread
		[ThreadStatic] private static byte[] _staticBuffer;
		
		private static byte[] GetStaticBuffer() => _staticBuffer ??= new byte[staticBufferSize];

		public static int ReadFixedInt32( BufferStream stream ) => stream.Read<int>();

		public static void WriteFixedInt32( BufferStream stream, int i ) => stream.Write<int>(i);
		
		public static long ReadFixedInt64( BufferStream stream ) => stream.Read<long>();

		public static void WriteFixedInt64( BufferStream stream, long i ) => stream.Write<long>(i);
		
		public static float ReadSingle( BufferStream stream ) => stream.Read<float>();

		public static void WriteSingle( BufferStream stream, float f ) => stream.Write<float>(f);

		public static double ReadDouble( BufferStream stream ) => stream.Read<double>();

		public static void WriteDouble( BufferStream stream, double f ) => stream.Write<double>(f);

		public static unsafe string ReadString( BufferStream stream )
		{
			Profiler.BeginSample( "ProtoParser.ReadString" );
			
			int length = (int)ReadUInt32( stream );
			if ( length <= 0 )
			{
				Profiler.EndSample();
				return "";
			}

			string str;
			var bytes = stream.GetRange( length ).GetSpan();
			fixed ( byte* ptr = &bytes[0] )
			{
				str = Encoding.UTF8.GetString( ptr, length );
			}

			Profiler.EndSample();

			return str;
		}

		public static void WriteString( BufferStream stream, string val )
		{
			Profiler.BeginSample( "ProtoParser.WriteString" );

			var buffer = GetStaticBuffer();
			var len = Encoding.UTF8.GetBytes( val, 0, val.Length, buffer, 0 );

			WriteUInt32( stream, (uint)len );

			if ( len > 0 )
			{
				new Span<byte>( buffer, 0, len ).CopyTo( stream.GetRange( len ).GetSpan() );
			}
			
			Profiler.EndSample();
		}

		/// <summary>
		/// Reads a length delimited byte array into a new byte[]
		/// </summary>
		public static byte[] ReadBytes( BufferStream stream )
		{
			Profiler.BeginSample( "ProtoParser.ReadBytes" );

			// Only limit length when reading from network
			int length = (int)ReadUInt32( stream );

			//Bytes
			byte[] buffer = new byte[ length ];
			ReadBytesInto( stream, buffer, length );
			Profiler.EndSample();

			return buffer;
		}

		/// <summary>
		/// Read into a byte[] that is disposed when the object is returned to the pool
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ReadPooledBytes( BufferStream stream )
		{
			Profiler.BeginSample( "ProtoParser.ReadPooledBytes" );

			// Only limit length when reading from network
			int length = (int)ReadUInt32( stream );

			//Bytes
			byte[] buffer = BufferStream.Shared.ArrayPool.Rent( length );
			ReadBytesInto( stream, buffer, length );
			Profiler.EndSample();

			return new ArraySegment<byte>( buffer, 0, length );
		}

		private static void ReadBytesInto( BufferStream stream, byte[] buffer, int length )
		{
			stream.GetRange( length ).GetSpan().CopyTo( buffer );
		}

		/// <summary>
		/// Skip the next varint length prefixed bytes.
		/// Alternative to ReadBytes when the data is not of interest.
		/// </summary>
		public static void SkipBytes( BufferStream stream )
		{
			int length = (int)ReadUInt32( stream );
			stream.Skip( length );
		}
		
		/// <summary>
		/// Writes length delimited byte array
		/// </summary>
		public static void WriteBytes( BufferStream stream, byte[] val )
		{
			WriteUInt32( stream, (uint)val.Length );
			new Span<byte>( val ).CopyTo( stream.GetRange( val.Length ).GetSpan() );
		}

		public static void WritePooledBytes( BufferStream stream, ArraySegment<byte> segment )
		{
			if (segment.Array == null)
			{
				WriteUInt32( stream, 0 );
				return;
			}

			WriteUInt32( stream, (uint)segment.Count );
			new Span<byte>( segment.Array, segment.Offset, segment.Count ).CopyTo( stream.GetRange( segment.Count ).GetSpan() );
		}
	}
}

public static class ProtoStreamExtensions
{
	public static void WriteToStream(this SilentOrbit.ProtocolBuffers.IProto proto, Stream stream, bool lengthDelimited = false, int maxSizeHint = 2 * 1024 * 1024)
	{
		if (proto == null)
		{
			throw new ArgumentNullException(nameof(proto));
		}

		if (stream == null)
		{
			throw new ArgumentNullException(nameof(stream));
		}

		using var writer = Facepunch.Pool.Get<BufferStream>().Initialize();

		var (maxLength, lengthPrefixSize) = GetLengthPrefixSize(maxSizeHint);
		BufferStream.RangeHandle lengthRange = default;
		if (lengthDelimited)
		{
			lengthRange = writer.GetRange(lengthPrefixSize);
		}

		var start = writer.Position;
		proto.WriteToStream(writer);

		if (lengthDelimited)
		{
			var length = writer.Position - start;
			if (length > maxLength)
			{
				throw new InvalidOperationException($"Written proto exceeds maximum size hint (maxSizeHint={maxSizeHint}, actualLength={length})");
			}
			
			var lengthSpan = lengthRange.GetSpan();
			var writtenBytes = ProtocolParser.WriteUInt32((uint)length, lengthSpan, 0);

			if (writtenBytes != lengthPrefixSize)
			{
				lengthSpan[writtenBytes - 1] |= 0x80; // mark the last written byte as having a continuation
				
				while (writtenBytes < lengthPrefixSize - 1)
				{
					lengthSpan[writtenBytes++] = 0x80; // continuation with no bits set
				}
				
				lengthSpan[writtenBytes] = 0; // and the last byte terminates the varint
			}
		}

		if (writer.Length > 0)
		{
			var buffer = writer.GetBuffer();
			stream.Write(buffer.Array, buffer.Offset, buffer.Count);
		}
	}
	
	private static (int MaxLength, int LengthPrefixSize) GetLengthPrefixSize(int maxSizeHint)
	{
		if (maxSizeHint < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(maxSizeHint));
		}

		if (maxSizeHint <= 0x7F) return (0x7F, 1);
		if (maxSizeHint <= 0x3FFF) return (0x3FFF, 2);
		if (maxSizeHint <= 0x1FFFFF) return (0x1FFFFF, 3);
		if (maxSizeHint <= 0xFFFFFFF) return (0xFFFFFF, 4);
		
		throw new ArgumentOutOfRangeException(nameof(maxSizeHint));
	}

	public static void ReadFromStream(this SilentOrbit.ProtocolBuffers.IProto proto, Stream stream, bool isDelta = false, int maxSize = 1 * 1024 * 1024)
	{
		if (proto == null)
		{
			throw new ArgumentNullException(nameof(proto));
		}

		if (stream == null)
		{
			throw new ArgumentNullException(nameof(stream));
		}

		var startPosition = stream.Position;
		
		var buffer = BufferStream.Shared.ArrayPool.Rent(maxSize);
		var offset = 0;
		var remaining = maxSize;
		while (remaining > 0)
		{
			var bytesRead = stream.Read(buffer, offset, remaining);
			if (bytesRead <= 0)
			{
				break;
			}

			offset += bytesRead;
			remaining -= bytesRead;
		}
		
		using var reader = Facepunch.Pool.Get<BufferStream>().Initialize(buffer, offset);
		proto.ReadFromStream(reader, isDelta);
		BufferStream.Shared.ArrayPool.Return(buffer);
		
		var protoReadLength = reader.Position;
		stream.Position = startPosition + protoReadLength;
	}

	public static void ReadFromStream(this SilentOrbit.ProtocolBuffers.IProto proto, Stream stream, int length, bool isDelta = false)
	{
		if (proto == null)
		{
			throw new ArgumentNullException(nameof(proto));
		}

		if (stream == null)
		{
			throw new ArgumentNullException(nameof(stream));
		}

		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(length));
		}

		var buffer = BufferStream.Shared.ArrayPool.Rent(length);
		var offset = 0;
		var remaining = length;
		while (remaining > 0)
		{
			var bytesRead = stream.Read(buffer, offset, remaining);
			if (bytesRead <= 0)
			{
				throw new InvalidOperationException("Unexpected end of stream");
			}

			offset += bytesRead;
			remaining -= bytesRead;
		}
		
		using var reader = Facepunch.Pool.Get<BufferStream>().Initialize(buffer, length);
		proto.ReadFromStream(reader, isDelta);
		
		BufferStream.Shared.ArrayPool.Return(buffer);
	}

	public static void ReadFromStreamLengthDelimited(this SilentOrbit.ProtocolBuffers.IProto proto, Stream stream, bool isDelta = false)
	{
		if (proto == null)
		{
			throw new ArgumentNullException(nameof(proto));
		}

		if (stream == null)
		{
			throw new ArgumentNullException(nameof(stream));
		}
		
		var length = (int)ProtocolParser.ReadUInt32(stream);
		ReadFromStream(proto, stream, length, isDelta);
	}
	
	public static byte[] ToProtoBytes(this SilentOrbit.ProtocolBuffers.IProto proto)
	{
		if (proto == null)
		{
			throw new ArgumentNullException(nameof(proto));
		}

		using var writer = Facepunch.Pool.Get<BufferStream>().Initialize();
		proto.WriteToStream(writer);
		
		var buffer = writer.GetBuffer();
		var bytes = new byte[writer.Position];
		new Span<byte>(buffer.Array, buffer.Offset, buffer.Count).CopyTo(bytes);
		return bytes;
	}
}
#endregion
#region ProtocolParserExceptions
//
// Exception used in the generated code
//

namespace SilentOrbit.ProtocolBuffers
{
	///<summary>>
	/// This exception is thrown when badly formatted protocol buffer data is read.
	///</summary>
	public class ProtocolBufferException : Exception
	{
		public ProtocolBufferException(string message) : base(message)
		{
		}
	}
}

#endregion
#region ProtocolParserKey
//
//  Reader/Writer for field key
//

namespace SilentOrbit.ProtocolBuffers
{
	public enum Wire
	{
		Varint = 0,		  //int32, int64, UInt32, UInt64, SInt32, SInt64, bool, enum
		Fixed64 = 1,		 //fixed64, sfixed64, double
		LengthDelimited = 2, //string, bytes, embedded messages, packed repeated fields
		//Start = 3,		 //  groups (deprecated)
		//End = 4,		   //  groups (deprecated)
		Fixed32 = 5,		 //32-bit	fixed32, SFixed32, float
	}

	public struct Key
	{
		public uint Field { get; set; }

		public Wire WireType { get; set; }

		public Key(uint field, Wire wireType)
		{
			this.Field = field;
			this.WireType = wireType;
		}

		public override string ToString()
		{
			return string.Format("[Key: {0}, {1}]", Field, WireType);
		}
	}

	public static partial class ProtocolParser
	{

		public static Key ReadKey(BufferStream stream)
		{
			uint n = ReadUInt32(stream);
			return new Key(n >> 3, (Wire)(n & 0x07));
		}

		public static Key ReadKey(byte firstByte, BufferStream stream)
		{
			if (firstByte < 128)
				return new Key((uint)(firstByte >> 3), (Wire)(firstByte & 0x07));
			uint fieldID = ((uint)ReadUInt32(stream) << 4) | ((uint)(firstByte >> 3) & 0x0F);
			return new Key(fieldID, (Wire)(firstByte & 0x07));
		}

		public static void WriteKey(BufferStream stream, Key key)
		{
			uint n = (key.Field << 3) | ((uint)key.WireType);
			WriteUInt32(stream, n);
		}

		/// <summary>
		/// Seek past the value for the previously read key.
		/// </summary>
		public static void SkipKey(BufferStream stream, Key key)
		{
			switch (key.WireType)
			{
				case Wire.Fixed32:
					stream.Skip(4);
					return;
				case Wire.Fixed64:
					stream.Skip(8);
					return;
				case Wire.LengthDelimited:
					stream.Skip((int)ProtocolParser.ReadUInt32(stream));
					return;
				case Wire.Varint:
					ProtocolParser.ReadSkipVarInt(stream);
					return;
				default:
					throw new NotImplementedException("Unknown wire type: " + key.WireType);
			}
		}
	}
}

#endregion
#region ProtocolParserMemory
//using System.Collections.Concurrent;

/// <summary>
/// MemoryStream management
/// </summary>
namespace SilentOrbit.ProtocolBuffers
{
	
}

#endregion
#region ProtocolParserVarInt

namespace SilentOrbit.ProtocolBuffers
{
	public static partial class ProtocolParser
	{
		/// <summary>
		/// Reads past a varint for an unknown field.
		/// </summary>
		public static void ReadSkipVarInt(BufferStream stream)
		{
			while (true)
			{
				int b = stream.ReadByte();
				if (b < 0)
					throw new IOException("Stream ended too early");

				if ((b & 0x80) == 0)
					return; //end of varint
			}
		}

		/// <summary>
		/// Unsigned VarInt format
		/// Do not use to read int32, use ReadUint64 for that.
		/// </summary>
		public static uint ReadUInt32( Span<byte> array, int pos, out int length )
		{
			int b;
			uint val = 0;
			length = 0;

			for (int n = 0; n < 5; n++)
			{
				length++;

				if (pos >= array.Length)
				{
					break;
				}
				b = array[ pos++ ];
				if (b < 0)
					throw new IOException( "Stream ended too early" );

				//Check that it fits in 32 bits
				if ((n == 4) && (b & 0xF0) != 0)
					throw new ProtocolBufferException( "Got larger VarInt than 32bit unsigned" );
				//End of check

				if ((b & 0x80) == 0)
					return val | (uint)b << (7 * n);

				val |= (uint)(b & 0x7F) << (7 * n);
			}

			throw new ProtocolBufferException( "Got larger VarInt than 32bit unsigned" );
		}

		/// <summary>
		/// Unsigned VarInt format
		/// </summary>
		public static int WriteUInt32( uint val, Span<byte> array, int pos )
		{
			int length = 0;
			byte b;
			while (pos < array.Length)
			{
				length++;
				b = (byte)(val & 0x7F);
				val = val >> 7;
				if (val == 0)
				{
					array[ pos++ ] = b;
					break;
				}
				else
				{
					b |= 0x80;
					array[ pos++ ] = b;
				}
			}
			return length;
		}

		#region VarInt: int32, uint32, sint32

		/// <summary>
		/// Zig-zag signed VarInt format
		/// </summary>
		public static int ReadZInt32(BufferStream stream)
		{
			uint val = ReadUInt32(stream);
			return (int)(val >> 1) ^ ((int)(val << 31) >> 31);
		}

		/// <summary>
		/// Zig-zag signed VarInt format
		/// </summary>
		public static void WriteZInt32(BufferStream stream, int val)
		{
			WriteUInt32(stream, (uint)((val << 1) ^ (val >> 31)));
		}

		/// <summary>
		/// Unsigned VarInt format
		/// Do not use to read int32, use ReadUint64 for that.
		/// </summary>
		public static uint ReadUInt32(BufferStream stream)
		{
			int b;
			uint val = 0;

			for (int n = 0; n < 5; n++)
			{
				b = stream.ReadByte();
				if (b < 0)
					throw new IOException("Stream ended too early");

				//Check that it fits in 32 bits
				if ((n == 4) && (b & 0xF0) != 0)
					throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
				//End of check

				if ((b & 0x80) == 0)
					return val | (uint)b << (7 * n);

				val |= (uint)(b & 0x7F) << (7 * n);
			}

			throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
		}

		/// <summary>
		/// Unsigned VarInt format
		/// </summary>
		public static void WriteUInt32(BufferStream stream, uint val)
		{
			byte b;
			while (true)
			{
				b = (byte)(val & 0x7F);
				val = val >> 7;
				if (val == 0)
				{
					stream.WriteByte(b);
					break;
				}
				else
				{
					b |= 0x80;
					stream.WriteByte(b);
				}
			}
		}

		/// <summary>
		/// Unsigned VarInt format
		/// Do not use to read int32, use ReadUint64 for that.
		/// </summary>
		public static uint ReadUInt32(Stream stream)
		{
			int b;
			uint val = 0;

			for (int n = 0; n < 5; n++)
			{
				b = stream.ReadByte();
				if (b < 0)
					throw new IOException("Stream ended too early");

				//Check that it fits in 32 bits
				if ((n == 4) && (b & 0xF0) != 0)
					throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
				//End of check

				if ((b & 0x80) == 0)
					return val | (uint)b << (7 * n);

				val |= (uint)(b & 0x7F) << (7 * n);
			}

			throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
		}

		/// <summary>
		/// Unsigned VarInt format
		/// </summary>
		public static void WriteUInt32(Stream stream, uint val)
		{
			byte b;
			while (true)
			{
				b = (byte)(val & 0x7F);
				val = val >> 7;
				if (val == 0)
				{
					stream.WriteByte(b);
					break;
				}
				else
				{
					b |= 0x80;
					stream.WriteByte(b);
				}
			}
		}

		#endregion

		#region VarInt: int64, UInt64, SInt64

		/// <summary>
		/// Zig-zag signed VarInt format
		/// </summary>
		public static long ReadZInt64(BufferStream stream)
		{
			ulong val = ReadUInt64(stream);
			return (long)(val >> 1) ^ ((long)(val << 63) >> 63);
		}

		/// <summary>
		/// Zig-zag signed VarInt format
		/// </summary>
		public static void WriteZInt64(BufferStream stream, long val)
		{
			WriteUInt64(stream, (ulong)((val << 1) ^ (val >> 63)));
		}

		/// <summary>
		/// Unsigned VarInt format
		/// </summary>
		public static ulong ReadUInt64(BufferStream stream)
		{
			int b;
			ulong val = 0;

			for (int n = 0; n < 10; n++)
			{
				b = stream.ReadByte();
				if (b < 0)
					throw new IOException("Stream ended too early");

				//Check that it fits in 64 bits
				if ((n == 9) && (b & 0xFE) != 0)
					throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
				//End of check

				if ((b & 0x80) == 0)
					return val | (ulong)b << (7 * n);

				val |= (ulong)(b & 0x7F) << (7 * n);
			}

			throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
		}

		/// <summary>
		/// Unsigned VarInt format
		/// </summary>
		public static void WriteUInt64(BufferStream stream, ulong val)
		{
			byte b;
			while (true)
			{
				b = (byte)(val & 0x7F);
				val = val >> 7;
				if (val == 0)
				{
					stream.WriteByte(b);
					break;
				}
				else
				{
					b |= 0x80;
					stream.WriteByte(b);
				}
			}
		}

		/// <summary>
		/// Unsigned VarInt format
		/// </summary>
		public static ulong ReadUInt64(Span<byte> array, int pos, out int length)
		{
			int b;
			ulong val = 0;
			length = 0;

			for (int n = 0; n < 10; n++)
			{
				length++;
				
				b = array[ pos++ ];
				if (b < 0)
					throw new IOException( "Stream ended too early" );

				//Check that it fits in 64 bits
				if ((n == 9) && (b & 0xFE) != 0)
					throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
				//End of check

				if ((b & 0x80) == 0)
					return val | (ulong)b << (7 * n);

				val |= (ulong)(b & 0x7F) << (7 * n);
			}

			throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
		}

		/// <summary>
		/// Unsigned VarInt format
		/// </summary>
		public static int WriteUInt64(ulong val, Span<byte> buffer, int pos)
		{
			int len = 0;
			while (true)
			{
				len++;
				byte b = (byte)(val & 0x7F);
				val = val >> 7;
				if (val == 0)
				{
					buffer[pos] = b;
					break;
				}
				else
				{
					b |= 0x80;
					buffer[pos++] = b;
				}
			}
			return len;
		}

		#endregion

		#region Varint: bool

		public static bool ReadBool(BufferStream stream)
		{
			int b = stream.ReadByte();
			if (b < 0)
				throw new IOException("Stream ended too early");
			if (b == 1)
				return true;
			if (b == 0)
				return false;
			throw new ProtocolBufferException("Invalid boolean value");
		}

		public static void WriteBool(BufferStream stream, bool val)
		{
			stream.WriteByte(val ? (byte)1 : (byte)0);
		}

		#endregion
	}
}
#endregion
#region BufferStream
public sealed partial class BufferStream : IDisposable, Facepunch.Pool.IPooled
{
	// Putting this in a nested class to avoid IL2CPP overhead for classes with static constructors
	public static class Shared
	{
		public static int StartingCapacity = 64;
		public static int MaximumCapacity = 512 * 1024 * 1024;
		public static int MaximumPooledSize = 64 * 1024 * 1024;
		public static readonly Facepunch.ArrayPool<byte> ArrayPool = new(MaximumPooledSize);
	}
	
	private bool _isBufferOwned;
	private byte[] _buffer;
	private int _length;
	private int _position;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _length;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value));
			}
			if (_position > value)
			{
				throw new InvalidOperationException($"Cannot shrink buffer below current position!");
			}

			var growSize = value - _length;
			if (growSize > 0)
			{
				EnsureCapacity(growSize);
			}

			_length = value;
		}
	}
	
	public int Position
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _position;
		set
		{
			if (value < 0 || value > _length)
			{
				throw new ArgumentOutOfRangeException(nameof(value));
			}

			_position = value;
		}
	}

	public BufferStream Initialize()
	{
		_isBufferOwned = true;
		_buffer = null;
		_length = 0;
		_position = 0;
		return this;
	}

	public BufferStream Initialize(Span<byte> buffer)
	{
		_isBufferOwned = true; // we need to copy the data into our own buffer
		_buffer = null;
		_length = buffer.Length;
		_position = 0;

		EnsureCapacity(buffer.Length);
		buffer.CopyTo(_buffer);
		
		return this;
	}
	
	public BufferStream Initialize(byte[] buffer, int length = -1)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException(nameof(buffer));
		}
		
		if (length > buffer.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(length));
		}
		
		_isBufferOwned = false;
		_buffer = buffer;
		_length = length < 0 ? buffer.Length : length;
		_position = 0;
		return this;
	}

	public void Dispose()
	{
		if (_isBufferOwned && _buffer != null)
		{
			ReturnBuffer(_buffer);
		}
		
		_buffer = null;

		var instance = this;
		Facepunch.Pool.Free(ref instance);
	}

	void Facepunch.Pool.IPooled.EnterPool()
	{
		if (_isBufferOwned && _buffer != null)
		{
			ReturnBuffer(_buffer);
		}
		
		_buffer = null;
	}
	
	void Facepunch.Pool.IPooled.LeavePool()
	{
	}

	public void Clear()
	{
		_length = 0;
		_position = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ReadByte()
	{
		if (_position >= _length)
		{
			return -1;
		}

		return _buffer[_position++];
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteByte(byte b)
	{
		EnsureCapacity(1);
		_buffer[_position++] = b;
		_length = Math.Max(_length, _position);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Read<T>() where T : unmanaged
	{
		var size = Unsafe.SizeOf<T>();
		if (_length - _position < size)
		{
			ThrowReadOutOfBounds();
		}

		ref readonly var value = ref Unsafe.As<byte, T>(ref _buffer[_position]);
		_position += size;
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Peek<T>() where T : unmanaged
	{
		var size = Unsafe.SizeOf<T>();
		if (_length - _position < size)
		{
			ThrowReadOutOfBounds();
		}

		ref readonly var value = ref Unsafe.As<byte, T>(ref _buffer[_position]);
		return value;
	}
	
	// Separate method to help with inlining of callers (throw expressions don't inline well)
	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ThrowReadOutOfBounds()
	{
		throw new InvalidOperationException("Attempted to read past the end of the BufferStream");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write<T>(T value) where T : unmanaged
	{
		var size = Unsafe.SizeOf<T>();
		EnsureCapacity(size);
		Unsafe.As<byte, T>(ref _buffer[_position]) = value;
		_position += size;
		_length = Math.Max(_length, _position);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RangeHandle GetRange(int count)
	{
		EnsureCapacity(count);
		var handle = new RangeHandle(this, _position, count);
		_position += count;
		_length = Math.Max(_length, _position);
		return handle;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Skip(int count)
	{
		 // todo: bounds checks?
		_position += count;
	}

	public ArraySegment<byte> GetBuffer()
	{
		if (_length == 0)
		{
			return new ArraySegment<byte>(Array.Empty<byte>(), 0, 0);
		}

		return new ArraySegment<byte>(_buffer, 0, _length);
	}
	
	private void EnsureCapacity(int spaceRequired)
	{
		if (spaceRequired < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(spaceRequired));
		}

		if (_buffer == null)
		{
			if (!_isBufferOwned)
			{
				throw new InvalidOperationException("Cannot allocate for BufferStream that doesn't own the buffer (did you forget to call Initialize?)");
			}
			
			var initialRequiredCapacity = spaceRequired <= Shared.StartingCapacity
				? Shared.StartingCapacity
				: spaceRequired;
			var capacity = Mathf.NextPowerOfTwo(initialRequiredCapacity);

			if (capacity > Shared.MaximumCapacity)
			{
				throw new Exception($"Preventing BufferStream buffer from growing too large (requiredLength={initialRequiredCapacity})");
			}

			_buffer = RentBuffer(capacity);
			return;
		}

		if (_buffer.Length - _position >= spaceRequired)
		{
			return;
		}

		var requiredLength = _position + spaceRequired;
		var newCapacity = Mathf.NextPowerOfTwo(Math.Max(requiredLength, _buffer.Length));
		
		if (!_isBufferOwned)
		{
			throw new InvalidOperationException($"Cannot grow buffer for BufferStream that doesn't own the buffer (requiredLength={requiredLength})");
		}
		
		if (newCapacity > Shared.MaximumCapacity)
		{
			throw new Exception($"Preventing BufferStream buffer from growing too large (requiredLength={requiredLength})");
		}

		var newBuffer = RentBuffer(newCapacity);
		Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _length);
		ReturnBuffer(_buffer);
		_buffer = newBuffer;
	}

	private static byte[] RentBuffer(int minSize)
	{
		if (minSize > Shared.MaximumPooledSize)
		{
			return new byte[minSize];
		}
		
		return Shared.ArrayPool.Rent(minSize);
	}

	private static void ReturnBuffer(byte[] buffer)
	{
		if (buffer == null ||
			buffer.Length > Shared.MaximumPooledSize)
		{
			return;
		}
		
		Shared.ArrayPool.Return(buffer);
	}
	
	public readonly ref struct RangeHandle
	{
		private readonly BufferStream _stream;
		private readonly int _offset;
		private readonly int _length;

		public RangeHandle(BufferStream stream, int offset, int length)
		{
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			_stream = stream ?? throw new ArgumentNullException(nameof(stream));
			_offset = offset;
			_length = length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<byte> GetSpan()
		{
			return new Span<byte>(_stream._buffer, _offset, _length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ArraySegment<byte> GetSegment()
		{
			return new ArraySegment<byte>(_stream._buffer, _offset, _length);
		}
	}
}
#endregion
