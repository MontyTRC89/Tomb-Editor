using System;
using System.IO;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.IO
{
    /// <summary>
    /// Raw payload of a compiled TombEngine (.ten) level: the compiler version stamp and the two
    /// decompressed blocks (media = textures/sounds, geometry = rooms/objects).
    /// </summary>
    public sealed class TenLevelFile
    {
        public Version Version { get; }
        public byte[] MediaData { get; }      // decompressed: textures + sounds
        public byte[] GeometryData { get; }   // decompressed: rooms + objects (sequential)

        public TenLevelFile(Version version, byte[] mediaData, byte[] geometryData)
        {
            Version = version;
            MediaData = mediaData;
            GeometryData = geometryData;
        }
    }

    /// <summary>
    /// Reads a compiled TombEngine (.ten) level. Mirrors, in reverse, the writer in
    /// <c>LevelCompilerTombEngine.WriteLevelTombEngine</c>.
    ///
    /// SLICE 1: file header, compiler version and the two LZ4-compressed payload blocks. The actual
    /// reconstruction of moveables/statics from the geometry block is layered on top of this in later
    /// slices; the format is version-stamped and currently only the current writer's layout is parsed.
    /// </summary>
    public static class TenLevelReader
    {
        // "TEN\0" signature written at the start of every compiled level.
        private static readonly byte[] Magic = { 0x54, 0x45, 0x4E, 0x00 };

        public static TenLevelFile ReadRaw(string fileName)
        {
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            var magic = reader.ReadBytes(4);
            if (magic.Length != 4 || magic[0] != Magic[0] || magic[1] != Magic[1] || magic[2] != Magic[2] || magic[3] != Magic[3])
                throw new InvalidDataException("Not a TombEngine (.ten) level: missing 'TEN' signature.");

            // Compiler version stamp (major, minor, build, reserved).
            var v = reader.ReadBytes(4);
            var version = new Version(v[0], v[1], v[2]);

            // Versions below 2.0 used a different header layout and zlib (not chunked-LZ4) block compression.
            // They are a distinct legacy format that this reader does not handle.
            if (version.Major < 2)
                throw new InvalidDataException(
                    $"This .ten level was compiled by an older TombEngine version ({version}) and is not supported. " +
                    "Recompile the level with a current version of Tomb Editor.");

            reader.ReadInt32(); // hashed machine name (reserved, unused here)
            reader.ReadInt32(); // checksum (only used for the engine's fast-reload, unused here)

            // Media block (textures + sounds): uncompressed length, compressed size, then chunked LZ4 data.
            reader.ReadInt64(); // media uncompressed length (the decompressor is self-delimiting)
            reader.ReadInt64(); // media compressed size
            byte[] mediaData = LZ4.DecompressData(fs);

            // Geometry block (rooms + objects): uncompressed length, compressed size, then chunked LZ4 data.
            reader.ReadInt64(); // geometry uncompressed length
            reader.ReadInt64(); // geometry compressed size
            byte[] geometryData = LZ4.DecompressData(fs);

            return new TenLevelFile(version, mediaData, geometryData);
        }

        /// <summary>
        /// Full import pipeline: reads the compiled level, parses its object section and decodes the texture
        /// atlases, then rebuilds the moveables and static meshes into a fresh TombEngine <see cref="Wad2"/>.
        /// </summary>
        public static Wad2 ImportObjectsAsWad2(string fileName)
        {
            var file = ReadRaw(fileName);
            var objects = TenGeometryReader.ReadObjects(file);
            var media = TenMediaReader.Read(file);

            var wad = new Wad2 { GameVersion = TRVersion.Game.TombEngine };
            var converter = new TenWad2Converter(objects, media);
            converter.ConvertMoveables(wad);
            converter.ConvertStatics(wad);

            return wad;
        }
    }
}
