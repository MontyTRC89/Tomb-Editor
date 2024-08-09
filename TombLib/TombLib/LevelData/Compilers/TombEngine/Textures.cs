using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using NLog;
using Pfim;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.IO;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private List<ImageC> _spritesTexturesPages;

        private TextureFootStep.Type? GetTextureSound(bool isTriangle, TextureArea area)
        {
            LevelTexture texture = area.Texture as LevelTexture;
            if (area.TextureIsInvisible || area.TextureIsUnavailable || area.TextureIsDegenerate || texture == null)
                return null;

            // Top right position for now
            Vector2 topRight = Vector2.Min(Vector2.Min(area.TexCoord0, area.TexCoord1), isTriangle ? area.TexCoord2 : Vector2.Min(area.TexCoord2, area.TexCoord3));
            return texture.GetTextureSoundFromTexCoord(topRight);
        }

        private TextureFootStep.Type GetTextureSound(Room room, int x, int z)
        {
            Block sector = room.Blocks[x, z];

            TextureFootStep.Type? result0 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(BlockFace.Floor));
            if (result0.HasValue)
                return result0.Value;

            TextureFootStep.Type? result1 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(BlockFace.Floor_Triangle2));
            if (result1.HasValue)
                return result1.Value;

            return TextureFootStep.Type.Stone;
        }

        private ImageC GetSkyTexture()
        {
            ReportProgress(96, "Building sky texture");

            // Read sky texture
            string skyFileName = _level.Settings.MakeAbsolute(_level.Settings.SkyTextureFilePath);
            if (!string.IsNullOrEmpty(skyFileName) && !File.Exists(skyFileName))
            {
                _progressReporter.ReportWarn("Specified sky texture not found, using default.");
                skyFileName = null;
            }
            else
                ReportProgress(96, "Reading sky texture: " + skyFileName);

            return _level.Settings.LoadSkyTexture(skyFileName);
        }

        private void BuildSprites()
        {
            ReportProgress(59, "Building sprites");
            var spriteSequences = _level.Settings.WadGetAllSpriteSequences();

            // Add all sprites to the texture packer
            var spriteAllocator = new TombEngineSpriteAllocator(_limits[Limit.TexPageSize]);
            var spriteTextureIDs = new Dictionary<Hash, int>();
            foreach (var sprite in spriteSequences.Values.SelectMany(sequence => sequence.Sprites))
                if (!spriteTextureIDs.ContainsKey(sprite.Texture.Hash))
                    spriteTextureIDs.Add(sprite.Texture.Hash, spriteAllocator.GetOrAllocateTextureID(sprite.Texture));

            // Pack textures
            _spritesTexturesPages = spriteAllocator.PackTextures(_level.Settings);

            // Now build data structures
            var tempSequences = new List<tr_sprite_sequence>();
            var tempSprites = new List<TombEngineSpriteTexture>();
            var lastOffset = 0;

            foreach (var sequence in spriteSequences.Values)
            {
                var newSequence = new tr_sprite_sequence();
                newSequence.NegativeLength = (short)-sequence.Sprites.Count;
                newSequence.ObjectID = (int)sequence.Id.TypeId;
                newSequence.Offset = (short)lastOffset;

                foreach (var sprite in sequence.Sprites)
                {
                    var id = spriteTextureIDs[sprite.Texture.Hash];
                    if (id == -1)
                    {
                        _progressReporter.ReportWarn("Sprite #" + sequence.Sprites.IndexOf(sprite) +
                            " in sequence #" + sequence.Id.TypeId + " wasn't added: size is too big or coordinates are invalid.");
                        continue;
                    }

                    var packInfo = spriteAllocator.GetPackInfo(id);

                    int padding = _level.Settings.TexturePadding;

                    float x = (packInfo.Pos.X + padding) / (float)_spritesTexturesPages[packInfo.OutputTextureID].Width;
                    float y = (packInfo.Pos.Y + padding) / (float)_spritesTexturesPages[packInfo.OutputTextureID].Height;
                    float w = (sprite.Texture.Image.Width - 1) / (float)_spritesTexturesPages[packInfo.OutputTextureID].Width;
                    float h = (sprite.Texture.Image.Height - 1) / (float)_spritesTexturesPages[packInfo.OutputTextureID].Height;

                    var newTexture = new TombEngineSpriteTexture();

                    newTexture.Tile = packInfo.OutputTextureID;
                    newTexture.X1 = x;
                    newTexture.Y1 = y;
                    newTexture.X2 = x + w;
                    newTexture.Y2 = y;
                    newTexture.X3 = x + w;
                    newTexture.Y3 = y + h;
                    newTexture.X4 = x;
                    newTexture.Y4 = y + h;

                    tempSprites.Add(newTexture);
                }

                tempSequences.Add(newSequence);
                lastOffset += sequence.Sprites.Count;
            }

            _spriteSequences = tempSequences;
            _spriteTextures = tempSprites;

#if DEBUG
            for (int n = 0; n < _spritesTexturesPages.Count; n++)
            {
                using (var bmp = _spritesTexturesPages[n].ToBitmap())
                {
                    int atlasSize = bmp.Width;

                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        foreach (var sprite in tempSprites)
                        {
                            var spriteBoundingBox = new System.Drawing.Rectangle(
                                    (int)(sprite.X1 * _spritesTexturesPages[n].Width),
                                    (int)(sprite.Y1 * _spritesTexturesPages[n].Height),
                                    (int)((sprite.X2 - sprite.X1) * _spritesTexturesPages[n].Width),
                                    (int)((sprite.Y3 - sprite.Y2) * _spritesTexturesPages[n].Height));
                            g.DrawRectangle(System.Drawing.Pens.GreenYellow, spriteBoundingBox);
                        }
                        bmp.Save("OutputDebug\\SpritesAtlas" + n + ".png");
                    }
                }
            }
#endif 
        }

        static bool DoesTypeMatch(byte[] type, string value)
        {
            if (type[0] == value[0] &&
                type[1] == value[1] &&
                type[2] == value[2] &&
                type[3] == value[3])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static MemoryStream RemoveColorChunks(MemoryStream stream)
        {
            MemoryStream final = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] temp = new byte[8];
            stream.Read(temp, 0, 8);
            final.Write(temp, 0, 8);

            while (true)
            {
                byte[] lenBytes = new byte[4];
                if (stream.Read(lenBytes, 0, 4) != 4)
                {
                    break;
                }

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lenBytes);
                }

                int len = BitConverter.ToInt32(lenBytes, 0);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lenBytes);
                }

                byte[] type = new byte[4];
                stream.Read(type, 0, 4);

                byte[] data = new byte[len + 4];
                stream.Read(data, 0, data.Length);

                if (!(DoesTypeMatch(type, "cHRM") ||
                    DoesTypeMatch(type, "gAMA") ||
                    DoesTypeMatch(type, "iCCP") ||
                    DoesTypeMatch(type, "sRGB")))
                {
                    final.Write(lenBytes, 0, lenBytes.Length);
                    final.Write(type, 0, type.Length);
                    final.Write(data, 0, data.Length);
                }
            }

            return final;
        }

        private byte[] GetUncompressedTexture(ImageC i)
        {
            MemoryStream output = new MemoryStream();
            i.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            output = RemoveColorChunks(output);
            return output.ToArray();
        }

        private byte[] GetCompressedTexture(ImageC i, CompressionFormat format)
        {
            using Image bitmap = i.ToBitmap();

            BcEncoder encoder = new BcEncoder();

            encoder.OutputOptions.GenerateMipMaps = true;
            encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
            encoder.OutputOptions.Format = format;
            encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;  

            MemoryStream output = new MemoryStream();
            encoder.EncodeToStream(i.ToByteArray(), i.Width, i.Height, PixelFormat.Bgra32, output);

            return output.ToArray();    
        }

        void WriteTextureData(BinaryWriterEx writer)
        {
            ReportProgress(90, "Writing texture data...");

            WriteAtlas(writer, _textureInfoManager.RoomsAtlas);
            WriteAtlas(writer, _textureInfoManager.MoveablesAtlas);
            WriteAtlas(writer, _textureInfoManager.StaticsAtlas);
            WriteAtlas(writer, _textureInfoManager.AnimatedAtlas);

            // Sprites textures
            writer.Write(_spritesTexturesPages.Count);
            foreach (var atlas in _spritesTexturesPages)
            {
                writer.Write(atlas.Width);
                writer.Write(atlas.Height);
                using (var ms = new MemoryStream())
                {
                    byte[] output = 
                        _level.Settings.CompressTextures ? 
                        GetCompressedTexture(atlas, CompressionFormat.Bc3) : 
                        GetUncompressedTexture(atlas);
                    writer.Write((int)output.Length);
                    writer.Write(output.ToArray());
                }
            }

            // Sky texture
            var sky = GetSkyTexture();
            using (var ms = new MemoryStream())
            {
                writer.Write(sky.Width);
                writer.Write(sky.Height);
                byte[] output =
                    _level.Settings.CompressTextures ?
                    GetCompressedTexture(sky, CompressionFormat.Bc3) :
                    GetUncompressedTexture(sky);
                writer.Write((int)output.Length);
                writer.Write(output.ToArray());
            }
        }

        void WriteAtlas(BinaryWriterEx writer, List<TombEngineAtlas> atlasList)
        {
            writer.Write(atlasList.Count);
            foreach (var atlas in atlasList)
            {
                writer.Write(atlas.ColorMap.Width);
                writer.Write(atlas.ColorMap.Height);

                using (var ms = new MemoryStream())
                {
                    byte[] output =
                        _level.Settings.CompressTextures ?
                        GetCompressedTexture(atlas.ColorMap, CompressionFormat.Bc3) :
                        GetUncompressedTexture(atlas.ColorMap);
                    writer.Write((int)output.Length);
                    writer.Write(output.ToArray());
                }

                writer.Write(atlas.HasNormalMap);
                if (!atlas.HasNormalMap)
                    continue;

                using (var ms = new MemoryStream())
                {
                    byte[] output =
                        _level.Settings.CompressTextures ?
                        GetCompressedTexture(atlas.NormalMap, CompressionFormat.Bc5) :
                        GetUncompressedTexture(atlas.NormalMap);
                    writer.Write((int)output.Length);
                    writer.Write(output.ToArray());
                }
            }
        }
    }
}
