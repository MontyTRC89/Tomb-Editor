using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using TombLib.Utils;

namespace TombLib.IO
{
    public class ChunkWriter : IDisposable
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

        public ChunkWriter(byte[] magicNumber, Stream stream, Compression compression = Compression.None, int compressionLevel = ZLib.DefaultCompressionLevel)
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

        public ChunkWriter(byte[] magicNumber, BinaryWriterFast fastWriter)
        {
            fastWriter.Write(magicNumber, 0, magicNumber.Length);
            fastWriter.Write(BitConverter.GetBytes(0), 0, 4);    // @FIXME: where is the compression handling, as above?
            _writer = fastWriter;
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
            private ChunkWriter _chunkWriter;
            private long _chunkSizePosition;
            private long _previousPosition;
            private long _maximumSize;

            public ChunkWritingState(ChunkWriter chunkWriter, ChunkId chunkID, long maximumSize)
            {
                _chunkWriter = chunkWriter;

                // Write chunk ID
                chunkID.ToStream(chunkWriter._writer);

                // Write chunk size
                _chunkSizePosition = chunkWriter._writer.Position;
                LEB128.Write(chunkWriter._writer, 0, maximumSize);

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
                LEB128.Write(_chunkWriter._writer, chunkSize, _maximumSize);
                _chunkWriter._writer.Position = newPosition;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkWritingState WriteChunk(ChunkId chunkID, long maximumSize = LEB128.MaximumSize4Byte) => new ChunkWritingState(this, chunkID, maximumSize);

        public delegate void WriteChunkDelegate();

        public void WriteChunk(ChunkId chunkID, WriteChunkDelegate writeChunk, long maximumSize = LEB128.MaximumSize4Byte)
        {
            using (WriteChunk(chunkID, maximumSize))
                writeChunk();
        }

        public void WriteChunkWithChildren(ChunkId chunkID, WriteChunkDelegate writeChunk, long maximumSize = LEB128.MaximumSize4Byte)
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
        public void WriteChunkEmpty(ChunkId chunkID)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkBool(ChunkId chunkID, bool value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 1);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkArrayOfBytes(ChunkId chunkID, byte[] value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, value.Length);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkInt(ChunkId chunkID, long value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, LEB128.GetLength(_writer, value));
            LEB128.Write(_writer, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkFloat(ChunkId chunkID, double value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 8);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkVector2(ChunkId chunkID, Vector2 value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 8);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkVector3(ChunkId chunkID, Vector3 value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 12);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkVector4(ChunkId chunkID, Vector4 value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 16);
            _writer.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChunkString(ChunkId chunkID, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, data.Length);
            _writer.Write(data);
        }
    }
}
