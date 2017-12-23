using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.IO
{

    public class ChunkReader : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private BinaryReaderEx _reader;

        public ChunkReader(byte[] expectedMagicNumber, Stream stream)
        {
            // Check file type
            byte[] magicNumber = new byte[expectedMagicNumber.Length];
            stream.Read(magicNumber, 0, expectedMagicNumber.Length);
            if (!magicNumber.SequenceEqual(expectedMagicNumber))
                throw new NotSupportedException("The header of the file was unrecognizable. Most likely it is not a valid file.");

            // Check chunk type
            uint chunkVersion = new BinaryReader(stream).ReadUInt32();
            if ((chunkVersion & 0x7fffffffu) != 0)
                throw new NotSupportedException("Chunk version " + chunkVersion + " is not supported");

            if ((chunkVersion & 0x80000000u) != 0)
            {
                int compressedSize = new BinaryReader(stream).ReadInt32();
                byte[] compressedBuffer = new byte[compressedSize];
                stream.Read(compressedBuffer, 0, compressedSize);
                byte[] decompressedBuffer = ZLib.DecompressData(compressedBuffer);

                _reader = new BinaryReaderEx(new MemoryStream(decompressedBuffer));
            }
            else
            {
                _reader = new BinaryReaderEx(stream);
            }
        }

        public void Dispose()
        {}

        public BinaryReaderEx Raw => _reader;

        // Raise exceptions if there were any problems
        private static string GetLocoationStr(long chunkStart, long chunkSize, ChunkId chunkID)
        {
            return " at offset " + chunkStart + " with size " + chunkSize + ". " + chunkID;
        }

        public delegate bool ReadChunkDelegate(ChunkId chunkID, long chunkSize);
        public void ReadChunks(ReadChunkDelegate tryParseChunk)
        {
            do
            {
                ChunkId chunkID = ChunkId.FromStream(_reader);
                if (chunkID == ChunkId.Empty) // End reached
                    break;

                // Read up to a 64 bit number for the chunk size
                long chunkSize = LEB128.ReadLong(_reader);

                // Try loading chunk content
                long chunkStart = _reader.BaseStream.Position;
                bool chunkRecognized = false;
                Exception chunkException = null;
                try
                {
                    chunkRecognized = tryParseChunk(chunkID, chunkSize);
                }
                catch (OperationCanceledException)
                { // Don't actually keep going if it's an 'OperationCanceledException'
                    throw;
                }
                catch (Exception exc)
                {
                    chunkException = exc;
                }
                long readDataCount = _reader.BaseStream.Position - chunkStart;

                // Print messages for various problems that might have occurred while loading
                if (chunkException != null)
                    logger.Error(chunkException, "Chunk loading raised an exception" + GetLocoationStr(chunkStart, chunkSize, chunkID));
                else if (!chunkRecognized)
                    logger.Warn("Chunk not recognized" + GetLocoationStr(chunkStart, chunkSize, chunkID));
                else if (readDataCount > chunkSize)
                    logger.Error("More data was read than available (Read: " + readDataCount + " Available: " + chunkSize + ")" + GetLocoationStr(chunkStart, chunkSize, chunkID));
                else if (readDataCount < chunkSize)
                    logger.Warn("Not all the available data was read (Read: " + readDataCount + " Available: " + chunkSize + ")" + GetLocoationStr(chunkStart, chunkSize, chunkID));

                // Adjust _stream position if necessaary
                if (readDataCount != chunkSize)
                    _reader.BaseStream.Position = chunkStart + chunkSize;
            } while (true);
        }

        public void ReadChunks()
        {
            ReadChunks((id2, chunkSize2) => false);
        }

        public byte[] ReadChunkArrayOfBytes(long length)
        {
            return _reader.ReadBytes((int)length);
        }

        public bool ReadChunkBool(long length)
        {
            return _reader.ReadBoolean();
        }

        public long ReadChunkLong(long length)
        {
            return LEB128.ReadLong(_reader);
        }

        public int ReadChunkInt(long length)
        {
            return LEB128.ReadInt(_reader);
        }

        public uint ReadChunkUInt(long length)
        {
            return LEB128.ReadUInt(_reader);
        }

        public short ReadChunkShort(long length)
        {
            return LEB128.ReadShort(_reader);
        }

        public ushort ReadChunkUshort(long length)
        {
            return LEB128.ReadUShort(_reader);
        }

        public sbyte ReadChunkSByte(long length)
        {
            return LEB128.ReadSByte(_reader);
        }

        public byte ReadChunkByte(long length)
        {
            return LEB128.ReadByte(_reader);
        }

        public string ReadChunkString(long length)
        {
            byte[] data = new byte[length];
            _reader.Read(data, 0, checked((int)length));
            string value = Encoding.UTF8.GetString(data);
            return value;
        }

        public float ReadChunkFloat(long length)
        {
            if (length < 8)
                return _reader.ReadSingle();
            else
                return (float)_reader.ReadDouble();
        }

        public double ReadChunkDouble(long length)
        {
            if (length < 8)
                return _reader.ReadSingle();
            else
                return _reader.ReadDouble();
        }

        public Vector2 ReadChunkVector2(long length)
        {
            return _reader.ReadVector2();
        }

        public Vector3 ReadChunkVector3(long length)
        {
            return _reader.ReadVector3();
        }

        public Vector4 ReadChunkVector4(long length)
        {
            return _reader.ReadVector4();
        }
    }
}
