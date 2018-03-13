using System;
using System.IO;
using System.Numerics;
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

        private BinaryWriterEx _writer;
        private Compression _compression;
        private int _compressionLevel;
        private Stream _baseStream;

        public ChunkWriter(byte[] magicNumber, Stream stream, Compression compression = Compression.None, int compressionLevel = ZLib.DefaultCompressionLevel)
        {
            stream.Write(magicNumber, 0, magicNumber.Length);

            _compression = compression;
            _compressionLevel = compressionLevel;
            _baseStream = stream;

            switch (compression)
            {
                case Compression.None:
                    _writer = new BinaryWriterEx(stream);
                    _writer.Write(0);
                    break;
                case Compression.Zlib:
                    new BinaryWriter(stream).Write(0x80000000);
                    _writer = new BinaryWriterEx(new MemoryStream());
                    _baseStream = stream;
                    break;
                default:
                    throw new ArgumentException("compression");
            }
        }

        public void Dispose()
        {
            switch (_compression)
            {
                case Compression.Zlib:
                    try
                    {
                        _writer.BaseStream.Position = 0;
                        byte[] compressedData = ZLib.CompressData(_writer.BaseStream, _compressionLevel);
                        new BinaryWriter(_baseStream).Write(compressedData.Length);
                        _baseStream.Write(compressedData, 0, compressedData.Length);
                    }
                    finally
                    {
                        _writer.Dispose();
                    }
                    break;
            }
        }

        public BinaryWriterEx Raw => _writer;
        public Stream Stream => _writer.BaseStream;

        public delegate void WriteChunkDelegate();
        public void WriteChunk(ChunkId chunkID, WriteChunkDelegate writeChunk, long maximumSize = LEB128.MaximumSize4Byte)
        {
            // Write chunk ID
            chunkID.ToStream(_writer);

            // Write chunk size
            long chunkSizePosition = _writer.BaseStream.Position;
            LEB128.Write(_writer, 0, maximumSize);

            // Write chunk content
            long previousPosition = _writer.BaseStream.Position;
            writeChunk();
            long newPosition = _writer.BaseStream.Position;

            // Update chunk size
            long chunkSize = newPosition - previousPosition;
            _writer.BaseStream.Position = chunkSizePosition;
            LEB128.Write(_writer, chunkSize, maximumSize);
            _writer.BaseStream.Position = newPosition;
        }

        public void WriteChunkWithChildren(ChunkId chunkID, WriteChunkDelegate writeChunk, long maximumSize = LEB128.MaximumSize4Byte)
        {
            WriteChunk(chunkID, () =>
            {
                writeChunk();
                WriteChunkEnd();
            }, maximumSize);
        }

        public void WriteChunkEnd()
        {
            _writer.Write((byte)0);
        }

        public void WriteChunkEmpty(ChunkId chunkID)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 0);
        }

        public void WriteChunkBool(ChunkId chunkID, bool value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 1);
            _writer.Write(value);
        }

        public void WriteChunkArrayOfBytes(ChunkId chunkID, byte[] value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, value.Length);
            _writer.Write(value);
        }

        public void WriteChunkInt(ChunkId chunkID, long value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, LEB128.GetLength(_writer, value));
            LEB128.Write(_writer, value);
        }

        public void WriteChunkFloat(BinaryWriter _stream, ChunkId chunkID, float value)
        {
            chunkID.ToStream(_stream);
            LEB128.Write(_stream, 4);
            _stream.Write(value);
        }

        public void WriteChunkFloat(ChunkId chunkID, double value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 8);
            _writer.Write(value);
        }

        public void WriteChunkVector2(ChunkId chunkID, Vector2 value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 8);
            _writer.Write(value);
        }

        public void WriteChunkVector3(ChunkId chunkID, Vector3 value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 12);
            _writer.Write(value);
        }

        public void WriteChunkVector4(ChunkId chunkID, Vector4 value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 16);
            _writer.Write(value);
        }

        public void WriteChunkMatrix4x4(ChunkId chunkID, Matrix4x4 value)
        {
            chunkID.ToStream(_writer);
            LEB128.Write(_writer, 64);
            _writer.Write(value);
        }

        public void WriteChunkString(ChunkId chunkID, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);

            chunkID.ToStream(_writer);
            LEB128.Write(_writer, data.Length);
            _writer.Write(data, 0, data.Length);
        }
    }
}
