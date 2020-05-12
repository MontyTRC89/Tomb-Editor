using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.IO
{
    public class SimplerChunkWriter : IDisposable
    {
        public enum Compression
        {
            None = 0,
            Zlib = 1
        }

        private readonly BinaryWriterFast _writer;
        private readonly Compression _compression;
        private readonly int _compressionLevel;
        private readonly Stream _baseStream;

        public SimplerChunkWriter(byte[] magicNumber, Stream stream, Compression compression = Compression.None, int compressionLevel = ZLib.DefaultCompressionLevel)
        {
            stream.Write(magicNumber, 0, magicNumber.Length);
            _compression = compression;
            _compressionLevel = compressionLevel;
            _baseStream = stream;

            switch (compression)
            {
                case Compression.None:
                    new BinaryWriter(stream).Write((uint)0);
                    _writer = new BinaryWriterFast(stream);
                    break;
                case Compression.Zlib:
                    new BinaryWriter(stream).Write((uint)0x80000000);
                    _writer = new BinaryWriterFast(new MemoryStream());
                    break;
                default:
                    throw new ArgumentException("compression");
            }
        }

        public void Dispose()
        {
            if (_baseStream == null)
                return;

            try
            {
                switch (_compression)
                {
                    case Compression.Zlib:
                        byte[] compressedData = ZLib.CompressData(_writer.BaseStream, _compressionLevel);
                        new BinaryWriter(_baseStream).Write(compressedData.Length);
                        _baseStream.Write(compressedData, 0, compressedData.Length);
                        break;
                }
            }
            finally
            {
                _writer.Dispose();
            }
        }

        public BinaryWriterFast Raw => _writer;

        public struct ChunkWritingState : IDisposable
        {
            private SimplerChunkWriter _chunkWriter;
            private long _chunkSizePosition;
            private long _previousPosition;
            private long _maximumSize;

            public ChunkWritingState(SimplerChunkWriter chunkWriter, int chunkID, long maximumSize)
            {
                _chunkWriter = chunkWriter;

                // Write chunk ID
                chunkWriter._writer.Write((int)chunkID);

                // Write chunk size
                _chunkSizePosition = chunkWriter._writer.Position;
                chunkWriter._writer.Write((long)maximumSize);

                // Prepare for writeing chunk content
                _previousPosition = chunkWriter._writer.Position;
                _maximumSize = maximumSize;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                // Update chunk size
                long newPosition = _chunkWriter._writer.Position;
                long chunkSize = newPosition - _previousPosition;
                _chunkWriter._writer.Position = _chunkSizePosition;
                _chunkWriter._writer.Write((long)chunkSize);
                _chunkWriter._writer.Position = newPosition;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkWritingState WriteChunk(int chunkID, long maximumSize = long.MaxValue) => new ChunkWritingState(this, chunkID, maximumSize);

        public delegate void WriteChunkDelegate();

        public void WriteChunk(int chunkID, WriteChunkDelegate writeChunk, long maximumSize = long.MaxValue)
        {
            using (WriteChunk(chunkID, maximumSize))
                writeChunk();
        }

        public void WriteChunkWithChildren(int chunkID, WriteChunkDelegate writeChunk, long maximumSize = long.MaxValue)
        {
            using (WriteChunk(chunkID, maximumSize))
            {
                writeChunk();
                WriteChunkEnd();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkEnd()
        {
            _writer.Write((byte)0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkEmpty(int chunkID)
        {
            _writer.Write((int)chunkID);
            _writer.Write((byte)0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkBool(int chunkID, bool value)
        {
            _writer.Write((int)chunkID);
            _writer.Write((long)1);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkArrayOfBytes(int chunkID, byte[] value)
        {
            _writer.Write((int)chunkID);
            _writer.Write((long)value.Length);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkInt(int chunkID, long value)
        {
            _writer.Write((int)chunkID);
            _writer.Write((long)8);
            LEB128.Write(_writer, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkFloat(int chunkID, double value)
        {
            _writer.Write((int)chunkID);
            _writer.Write((long)8);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkVector2(int chunkID, Vector2 value)
        {
            _writer.Write((int)chunkID);
            _writer.Write((long)8);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkVector3(int chunkID, Vector3 value)
        {
            _writer.Write((int)chunkID);
            _writer.Write((long)12);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkVector4(int chunkID, Vector4 value)
        {
            _writer.Write((int)chunkID);
            _writer.Write((long)16);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkString(int chunkID, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            _writer.Write((int)chunkID);
            _writer.Write((long)data.Length);
            _writer.Write(data);
        }
    }
}
