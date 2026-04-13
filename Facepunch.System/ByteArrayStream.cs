using System;
using System.IO;

namespace Facepunch
{
    public class ByteArrayStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position
        {
            get => _position - _base;
            set => Seek(value, SeekOrigin.Begin);
        }

        private byte[] _data;
        private int _base;
        private int _length;
        private int _position; // includes offset

        public ByteArrayStream()
        {
            _data = Array.Empty<byte>();
            _base = 0;
            _length = 0;
        }

        public ByteArrayStream(byte[] data, int offset, int length)
        {
            SetData(data, offset, length);
        }

        public void SetData(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            _data = data;
            _base = offset;
            _length = length;
            _position = _base;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0 || count > buffer.Length - offset)
                throw new ArgumentOutOfRangeException(nameof(count));

            var end = Math.Min(_position + count, _base + _length);
            var bytesRead = end - _position;

            if (bytesRead <= 0)
                return 0;

            Buffer.BlockCopy(_data, _position, buffer, offset, bytesRead);
            _position += bytesRead;
            return bytesRead;
        }

        public override int ReadByte()
        {
            if (_position < _base || _position >= _base + _length)
                return -1;

            return _data[_position++];
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0 || count > buffer.Length - offset)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (_position + count >= _base + _length)
                throw new IOException("Tried to write beyond the buffer bounds");

            Buffer.BlockCopy(buffer, offset, _data, _position, count);
            _position += count;
        }

        public override void WriteByte(byte value)
        {
            if (_position < _base || _position >= _base + _length)
                throw new IOException("Tried to write beyond the buffer bounds");

            _data[_position++] = value;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            int newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                default:
                    newPosition = _base + (int)offset;
                    break;
                
                case SeekOrigin.Current:
                    newPosition = _position + (int)offset;
                    break;

                case SeekOrigin.End:
                    newPosition = (_base + _length) + (int)offset;
                    break;
            }

            if (newPosition < _base || newPosition > _base + _length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            _position = newPosition;
            return Position;
        }

        public override void Flush()
        {

        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
