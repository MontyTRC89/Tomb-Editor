using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.Wad
{
    partial class Wad2
    {
        public static Wad2 LoadFromWad2(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumber, fileStream))
                return LoadWad2(chunkIO, filename);
        }

        private static Wad2 LoadWad2(ChunkReader chunkIO, string thisPath)
        {
            var wad = new Wad2();

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (LoadTextures(chunkIO, id, wad))
                    return true;
                else if (LoadSprites(chunkIO, id, wad))
                    return true;
                else if (LoadMeshes(chunkIO, id, wad))
                    return true;
                else if (LoadSamples(chunkIO, id, wad))
                    return true;
                else if (LoadMoveables(chunkIO, id, wad))
                    return true;
                else if (LoadStatics(chunkIO, id, wad))
                    return true;
                else if (LoadSpriteSequences(chunkIO, id, wad))
                    return true;
                else if (LoadSounds(chunkIO, id, wad))
                    return true;

                return false;
            });

            return wad;
        }

        private static bool LoadTextures(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Textures)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Texture)
                    return false;

                var texture = new WadTexture();
                var width = LEB128.ReadInt(chunkIO.Raw);
                var height = LEB128.ReadInt(chunkIO.Raw);
                byte[] textureData = new byte[1];

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.TextureData)
                        textureData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                texture.Image = ImageC.FromByteArray(textureData, width, height);
                texture.UpdateHash();

                wad.Textures.Add(texture.Hash, texture);

                return true;
            });

            return true;
        }

        private static bool LoadSprites(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Sprites)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sprite)
                    return false;

                var texture = new WadSprite();
                var width = LEB128.ReadInt(chunkIO.Raw);
                var height = LEB128.ReadInt(chunkIO.Raw);
                byte[] textureData = new byte[1];

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.TextureData)
                        textureData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                texture.Image = ImageC.FromByteArray(textureData, width, height);
                texture.UpdateHash();

                wad.SpriteTextures.Add(texture.Hash, texture);

                return true;
            });

            return true;
        }

        private static bool LoadSamples(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }

        private static bool LoadMeshes(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }

        private static bool LoadMoveables(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }

        private static bool LoadStatics(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }

        private static bool LoadSpriteSequences(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }

        private static bool LoadSounds(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }
    }
}
