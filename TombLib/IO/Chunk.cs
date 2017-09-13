using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.IO
{
    // Chunks represent a hierarchical (so nested) data structure.
    // Each chunk contains first some data and then potentially more chunks depending on the documentation of each chunk.
    // When a stream of chunks is encountered, it is continously parsed the following way: 
    //     (The chunk stream is considered to be ended immediately after a ChunkIdLength of 0 is encountered.)
    //
    //     LEB128 unsigned int (up to 5 bytes)   ChunkIdLength
    //         The length of ChunkId
    //
    //     n bytes                               ChunkId
    //         n bytes that identificate the chunk.
    //
    //         Custom ChunkIds can be defined, but they are recommended to ASCII or UTF-8 strings
    //         to allow easy identification in a hex editor.
    //
    //         To avoid Id collisions (different people define the same chunk Ids to different things),
    //         it is recommended to begin all custom ChunkIds of some software with a unique prefix.
    //         (0x54, 0x65 ('Te') in case for Tomb Editor)
    //                                   
    //     LEB128 unsigned int (up to 9 bytes)   ChunkSize
    //         The size of the content of the chunk
    //
    //     n bytes                               ChunkContent
    //         n bytes that identificate the chunk.
    //         The meaning of those byte is entirely dependent on the previously read ChunkId.
    //         Applications are encouraged to ignore chunk content whose IDs they don't recognize.
    //
    //
    //
    // Key properties of the format:
    //     - Unknown chunks can be ignored to enable backwards as well as forwards compability.
    //     - Old chunk types can be replaced by new ones simply by choosing a new Id.
    //     - An empty chunks stream is only 1 byte big. (A single 0 byte, so that the process is aborted immediately like described above)
    //     - The minimal size overhead of introducing a new chunk is only 2 bytes + the byte length of the ChunkID
    //


    public struct ChunkID
    {
        public static readonly ChunkID Empty = new ChunkID(new byte[0], 0);
        private static readonly Encoding _encoding = Encoding.UTF8;

        private byte[] _idBytes; // Actually store the chunk ID as bytes. We don't want fancy Unicode comparison (different id bytes would compare identical, also big overhead)
        private int _idLength; // Extra field to allow only part of the array

        public ChunkID(byte[] idBytes)
        {
            _idBytes = idBytes;
            _idLength = idBytes.Length;
        }

        public ChunkID(byte[] idBytes, int idLength)
        {
            _idBytes = idBytes;
            _idLength = idLength;
        }

        public static ChunkID FromString(string str)
        {
            return new ChunkID(_encoding.GetBytes(str));
        }

        public static ChunkID FromStream(BinaryReaderEx stream)
        {
            int idLength = (int)(LEB128.ReadULong(stream));
            return new ChunkID(stream.ReadBytes(idLength), idLength); // If this turns out to be slow, we might want to kind of caching to reuse an array.
        }

        public void ToStream(BinaryWriterEx stream)
        {
            LEB128.WriteULong(stream, _idLength);
            stream.Write(_idBytes, 0, _idLength);
        }

        public static unsafe bool operator ==(ChunkID first, ChunkID second)
        {
            if (first._idLength != second._idLength)
                return false;

            // Compare byte arrays fast
            fixed (byte* firstPtr = first._idBytes)
            fixed (byte* secondPtr = second._idBytes)
                for (int i = 0; i < first._idLength; ++i)
                    if (firstPtr[i] != secondPtr[i])
                        return false;

            return true;
        }

        public static bool operator !=(ChunkID first, ChunkID second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            return this == (ChunkID)obj;
        }

        public override int GetHashCode()
        {
            int hash = unchecked(_idLength * (int)3239679517); // Random prime
            for (int i = 0; i < _idLength; ++i)
                hash = unchecked((hash * 1321196299) + _idBytes[i]); // Random prime
            return hash;
        }

        public string Name
        {
            get
            {
                try
                {
                    return _encoding.GetString(_idBytes, 0, _idLength);
                }
                catch
                {
                    return "Invalid UTF-8 sequence";
                }
            }
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder("Name: \"");
            str.Append(Name);
            str.Append("\", Hex: ");
            for (int i = 0; i < _idLength; ++i)
            {
                if (i != 0)
                    if ((i & 3) == 0)
                        str.Append("  '  ");
                    else
                        str.Append(" ");
                str.Append(_idBytes[i].ToString("X2"));
            }
            return str.ToString();
        }
    };

    public static class ChunkProcessing
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Raise exceptions if there were any problems
        private static string GetLocoationStr(long chunkStart, long chunkSize, ChunkID chunkID)
        {
            return " at offset " + chunkStart + " with size " + chunkSize + ". " + chunkID;
        }

        public delegate bool ParseChunkDelegate(BinaryReaderEx stream, ChunkID chunkID, long chunkSize);
        public static void ParseChunks(BinaryReaderEx stream, ParseChunkDelegate tryParseChunk)
        {
            do
            {
                ChunkID chunkID = ChunkID.FromStream(stream);
                if (chunkID == ChunkID.Empty) // End reached
                    break;

                // Read up to a 64 bit number for the chunk size
                long chunkSize = LEB128.ReadULong(stream);

                // Try loading chunk content
                long chunkStart = stream.BaseStream.Position;
                bool chunkRecognized = false;
                Exception chunkException = null;
                try
                {
                    chunkRecognized = tryParseChunk(stream, chunkID, chunkSize);
                }
                catch (OperationCanceledException)
                { // Don't actually keep going if it's an 'OperationCanceledException'
                    throw;
                }
                catch (Exception exc)
                {
                    chunkException = exc;
                }
                long readDataCount = stream.BaseStream.Position - chunkStart;

                // Print messages for various problems that might have occurred while loading
                if (chunkException != null)
                    logger.Error(chunkException, "Chunk loading raised an exception" + GetLocoationStr(chunkStart, chunkSize, chunkID));
                else if (!chunkRecognized)
                    logger.Warn("Chunk not recognized" + GetLocoationStr(chunkStart, chunkSize, chunkID));
                else if (readDataCount > chunkSize)
                    logger.Error("More data was read than available (Read: " + readDataCount + " Available: " + chunkSize + ")" + GetLocoationStr(chunkStart, chunkSize, chunkID));
                else if (readDataCount < chunkSize)
                    logger.Warn("Not all the available data was read (Read: " + readDataCount + " Available: " + chunkSize + ")" + GetLocoationStr(chunkStart, chunkSize, chunkID));

                // Adjust stream position if necessaary
                if (readDataCount != chunkSize)
                    stream.BaseStream.Position = chunkStart + chunkSize;
            } while (true);
        }

        public delegate void WriteChunkDelegate(BinaryWriterEx stream, ChunkID chunkID);
        public static void WriteChunk(BinaryWriterEx stream, ChunkID chunkID, WriteChunkDelegate writeChunk, long maximumSize = LEB128.MaximumSize4Byte)
        {
            // Write chunk ID
            chunkID.ToStream(stream);

            // Write chunk size (reserved for later)
            long chunkSizePosition = stream.BaseStream.Position;
            LEB128.WriteULong(stream, 0, maximumSize);

            // Write chunk content
            long previousPosition = stream.BaseStream.Position;
            writeChunk(stream, chunkID);
            long newPosition = stream.BaseStream.Position;

            // Update chunk size
            long chunkSize = newPosition - previousPosition;
            stream.BaseStream.Position = chunkSizePosition;
            LEB128.WriteULong(stream, chunkSize, maximumSize);
            stream.BaseStream.Position = newPosition;
        }

        public static void WriteChunkEnd(BinaryWriterEx stream)
        {
            stream.Write((byte)0);
        }
    }
}
