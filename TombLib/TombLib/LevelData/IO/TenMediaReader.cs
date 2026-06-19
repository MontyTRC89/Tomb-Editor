using System.Collections.Generic;
using System.IO;
using TombLib.Utils;

namespace TombLib.LevelData.IO
{
    /// <summary>
    /// Decoded color atlases extracted from the media block of a compiled <c>.ten</c> level. Only the
    /// color maps are needed to re-texture imported objects; normal/ORSH/emissive maps and sprite/sky
    /// pages are skipped. Atlas page indices are per-destination, matching the compiler's per-list
    /// AtlasIndex (baseIndex 0 for rooms/moveables/statics).
    /// </summary>
    public sealed class TenMediaData
    {
        public List<ImageC> RoomsAtlas = new();
        public List<ImageC> MoveablesAtlas = new();
        public List<ImageC> StaticsAtlas = new();
        public List<ImageC> AnimatedAtlas = new();
    }

    /// <summary>
    /// Reads the (already LZ4-decompressed) media block, mirroring <c>WriteTextureData</c>. Each image is
    /// stored either as a DDS (BC3/BC5, when texture compression is enabled) or a PNG; both are detected
    /// automatically by <see cref="ImageC.FromStream"/> from the blob's magic, so the writer's
    /// CompressTextures setting does not need to be known.
    /// </summary>
    public static class TenMediaReader
    {
        public static TenMediaData Read(TenLevelFile file)
        {
            using var ms = new MemoryStream(file.MediaData, writable: false);
            using var r = new BinaryReader(ms);

            // Order matches WriteTextureData: rooms, moveables, statics, animated, then sprites + sky.
            return new TenMediaData
            {
                RoomsAtlas = ReadAtlas(r),
                MoveablesAtlas = ReadAtlas(r),
                StaticsAtlas = ReadAtlas(r),
                AnimatedAtlas = ReadAtlas(r)
                // sprite pages and the sky texture follow but are not needed for object import.
            };
        }

        // WriteAtlas: int count; per atlas -> int Width, int Height, ColorMap image,
        //   bool hasNormal [+image], bool hasORSH [+image], bool hasEmissive [+image]
        private static List<ImageC> ReadAtlas(BinaryReader r)
        {
            int count = r.ReadInt32();
            var list = new List<ImageC>(count);

            for (int i = 0; i < count; i++)
            {
                r.ReadInt32(); // Width  (the decoder recovers real dimensions from the blob)
                r.ReadInt32(); // Height

                list.Add(ReadImage(r)); // ColorMap

                if (r.ReadBoolean()) SkipImage(r); // NormalMap  (BC5)
                if (r.ReadBoolean()) SkipImage(r); // ORSHMap
                if (r.ReadBoolean()) SkipImage(r); // EmissiveMap
            }

            return list;
        }

        // WriteImageFast: int byteLength; then the encoded image bytes (DDS or PNG).
        private static ImageC ReadImage(BinaryReader r)
        {
            int len = r.ReadInt32();
            byte[] blob = r.ReadBytes(len);
            using var ms = new MemoryStream(blob, writable: false);
            return ImageC.FromStream(ms);
        }

        private static void SkipImage(BinaryReader r)
        {
            int len = r.ReadInt32();
            r.BaseStream.Seek(len, SeekOrigin.Current);
        }
    }
}
