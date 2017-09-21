using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using TombEditor.Geometry;
using TombLib.IO;
using System.Drawing;
using System.Drawing.Imaging;
using TombLib.Utils;
using SharpDX;
using TombLib.Wad;
using System.Linq;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private void PrepareTextures()
        {
            List<ImageC> packedTextures = _objectTextureManager.PackTextures(_progressReporter);
            List<ImageC> spritePages = BuildSprites(packedTextures.Count);

            ReportProgress(10, "Building final texture map");

            byte[] texture32Data = new byte[(spritePages.Count + packedTextures.Count) * (256 * 256 * 4)];

            for (int i = 0; i < packedTextures.Count; ++i)
                packedTextures[i].RawCopyTo(texture32Data, i * (256 * 256 * 4));
            for (int i = 0; i < spritePages.Count; ++i)
                spritePages[i].RawCopyTo(texture32Data, (packedTextures.Count + i) * (256 * 256 * 4));

            _texture32Data = texture32Data;
        }

        private TextureSound? GetTextureSound(bool isTriangle, TextureArea area)
        {
            LevelTexture texture = area.Texture as LevelTexture;
            if (texture == null)
                return null;

            // Top right position for now
            Vector2 topRight = Vector2.Min(Vector2.Min(area.TexCoord0, area.TexCoord1), isTriangle ? area.TexCoord2 : Vector2.Min(area.TexCoord2, area.TexCoord3));
            return texture.GetTextureSoundFromTexCoord(topRight);
        }

        private TextureSound GetTextureSound(Room room, int x, int z)
        {
            Block sector = room.Blocks[x, z];

            TextureSound? result0 = GetTextureSound(!sector.FloorIsQuad, sector.GetFaceTexture(BlockFace.Floor));
            if (result0.HasValue)
                return result0.Value;

            TextureSound? result1 = GetTextureSound(!sector.FloorIsQuad, sector.GetFaceTexture(BlockFace.FloorTriangle2));
            if (result1.HasValue)
                return result1.Value;

            return TextureSound.Stone;
        }

        private Stream PrepareFontAndSkyTexture()
        {
            ReportProgress(18, "Building font & sky textures");

            var image = ImageC.CreateNew(256, 512);

            // Read font texture
            string fontFileName = _level.Settings.FontTextureFileNameAbsoluteOrDefault;
            ReportProgress(19, "Reading font texture: " + fontFileName);
            image.CopyFrom(0, 0, Geometry.IO.ResourceLoader.LoadRawExtraTexture(fontFileName)); 

            // Read sky texture
            string skyFileName = _level.Settings.SkyTextureFileNameAbsoluteOrDefault;
            ReportProgress(18, "Reading sky texture: " + skyFileName);
            image.CopyFrom(0, 256, Geometry.IO.ResourceLoader.LoadRawExtraTexture(skyFileName));
                
            return image.ToRawStream();
        }

        private List<ImageC> BuildSprites(int pagesBeforeSprites)
        {
            ReportProgress(9, "Building sprites");
            //ReportProgress(9, "Reading " + _level.Wad.OriginalWad.BaseName + ".swd");

            // Collect all sprites and sort them
            List<WadSprite> packedTextures = _level.Wad.SpriteTextures.Values.ToList();
            packedTextures.Sort(new ComparerWadTextures());

            // Pack the textures in pages of 256x256
            List<ImageC> texturePages = new List<ImageC>();
            ImageC currentTexture = ImageC.CreateNew(256, 256);
            RectPackerSimpleStack packer = new RectPackerSimpleStack(256, 256);
            List<WadTexture> currentTextures = new List<WadTexture>();
            WadTexture texture = new WadTexture();

            for (int processedTexture = 0; processedTexture <= packedTextures.Count; ++processedTexture)
            {
                RectPacker.Point? point = null;
                if (processedTexture < packedTextures.Count)
                {
                    texture = packedTextures[processedTexture];
                    point = packer.TryAdd(texture.Width, texture.Height);
                }

                // If no more textures can be added, it's time to end current page
                if (!point.HasValue || processedTexture == packedTextures.Count)
                {
                    foreach (var textureToSave in currentTextures)
                    {
                        int startX = (int)textureToSave.PositionInPackedTexture.X;
                        int startY = (int)textureToSave.PositionInPackedTexture.Y;

                        for (int y = 0; y < textureToSave.Height; y++)
                        {
                            for (int x = 0; x < textureToSave.Width; x++)
                            {
                                var color = textureToSave.Image.GetPixel(x, y);
                                currentTexture.SetPixel(startX + x, startY + y, color);
                            }
                        }
                    }

                    // Add the 256x256 page to the list
                    texturePages.Add(currentTexture);

                    if (processedTexture == packedTextures.Count) break;

                    // Create new packer and texture
                    packer = new RectPackerSimpleStack(256, 256);
                    currentTexture = ImageC.CreateNew(256, 256);
                    currentTextures = new List<WadTexture>();
                    
                    // Pack in new texture
                    point = packer.TryAdd(texture.Width, texture.Height);
                }

                texture.PositionInPackedTexture = new Vector2(point.Value.X, point.Value.Y);
                texture.Tile = (ushort)texturePages.Count;

                currentTextures.Add(texture);
            }

            // Now build data structures
            var tempSequences = new List<tr_sprite_sequence>();
            var tempSprites = new List<tr_sprite_texture>();
            var lastOffset = 0;

            foreach (var oldSequence in _level.Wad.SpriteSequences)
            {
                var newSequence = new tr_sprite_sequence();
                newSequence.NegativeLength = (short)-oldSequence.Sprites.Count;
                newSequence.ObjectID = (int)oldSequence.ObjectID;
                newSequence.Offset = (short)lastOffset;

                lastOffset += oldSequence.Sprites.Count;

                foreach (var oldTexture in oldSequence.Sprites)
                {
                    var newTexture = new tr_sprite_texture();

                    newTexture.TopSide = (short)oldTexture.PositionInPackedTexture.Y;
                    newTexture.LeftSide = (short)oldTexture.PositionInPackedTexture.X;
                    newTexture.X = (byte)newTexture.LeftSide;
                    newTexture.Y = (byte)newTexture.TopSide;
                    newTexture.Width = (ushort)((oldTexture.Width - 1) * 256);
                    newTexture.Height = (ushort)((oldTexture.Height - 1) * 256);
                    newTexture.RightSide = (short)(newTexture.LeftSide + (oldTexture.Width - 1));
                    newTexture.BottomSide = (short)(newTexture.TopSide + (oldTexture.Height - 1));
                    newTexture.Tile = (ushort)(pagesBeforeSprites + oldTexture.Tile);

                    tempSprites.Add(newTexture);
                }

                tempSequences.Add(newSequence);
            }

            _spriteSequences = tempSequences.ToArray();
            _spriteTextures = tempSprites.ToArray();

            return texturePages;

            // TODO: to remove
            /*
             using (var reader = new BinaryReaderEx(new FileStream(
                _level.Wad.OriginalWad.BasePath + Path.DirectorySeparatorChar + _level.Wad.OriginalWad.BaseName + ".swd",
                FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // Version
                reader.ReadUInt32();

                //Sprite texture array
                _spriteTextures = new tr_sprite_texture[reader.ReadUInt32()];
                for (int i = 0; i < _spriteTextures.Length; i++)
                {
                    byte[] buffer;
                    reader.ReadBlockArray(out buffer, 16);

                    _spriteTextures[i] = new tr_sprite_texture
                    {
                        Tile = (ushort)(pagesBeforeSprites), // TODO use correct page offset here.
                        X = buffer[0],
                        Y = buffer[1],
                        Width = (ushort)(buffer[5] * 256),
                        Height = (ushort)(buffer[7] * 256),
                        LeftSide = buffer[0],
                        TopSide = buffer[1],
                        RightSide = (short)(buffer[0] + buffer[5] + 1),
                        BottomSide = (short)(buffer[1] + buffer[7] + 1)
                    };
                }

                // Unknown value
                int spriteDataSize = reader.ReadInt32();

                // Load the real sprite texture data
                int numSpriteTexturePages = spriteDataSize / (65536 * 3);
                if ((spriteDataSize % (65536 * 3)) != 0)
                    numSpriteTexturePages++;
                
                for (int i = 0; i < numSpriteTexturePages; ++i)
                {
                    var spritePage = ImageC.CreateNew(256, 256);
                    for (int y = 0; y < 256; y++)
                        for (int x = 0; x < 256; x++)
                        {
                            byte r = reader.ReadByte();
                            byte g = reader.ReadByte();
                            byte b = reader.ReadByte();

                            if (r == 255 & g == 0 && b == 255)
                                spritePage.SetPixel(x, y, 0, 0, 0, 0);
                            else
                                spritePage.SetPixel(x, y, b, g, r, 255);
                        }
                    texturePages.Add(spritePage);
                }

                // Sprite sequences
                reader.ReadBlockArray(out _spriteSequences, reader.ReadUInt32());
            }
            return texturePages;*/
        }

        private static byte[] PackTextureMap32To16Bit(IReadOnlyList<byte> map, int width, int height)
        {
            var newMap = new byte[width * height * 2];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    ushort a1 = map[y * 256 * 4 + x * 4 + 3];
                    ushort r1 = map[y * 256 * 4 + x * 4 + 2];
                    ushort g1 = map[y * 256 * 4 + x * 4 + 1];
                    ushort b1 = map[y * 256 * 4 + x * 4 + 0];

                    var r = (ushort)(r1 / 8);
                    var g = (ushort)(g1 / 8);
                    var b = (ushort)(b1 / 8);
                    ushort a;

                    if (a1 == 0)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                        a = 0;
                    }
                    else
                    {
                        a = 0x8000;
                    }

                    if (r1 < 8)
                        r = 0;
                    if (g1 < 8)
                        g = 0;
                    if (b1 < 8)
                        b = 0;

                    ushort tmp = 0;

                    if (r1 == 255 && g1 == 255 && b1 == 255)
                    {
                        tmp = 0xffff;
                    }
                    else
                    {
                        tmp |= a;
                        tmp |= (ushort)(r << 10);
                        tmp |= (ushort)(g << 5);
                        tmp |= b;
                    }

                    newMap[y * 256 * 2 + 2 * x] = (byte)(tmp & 0xff);
                    newMap[y * 256 * 2 + 2 * x + 1] = (byte)((tmp & 0xff00) >> 8);
                }
            }

            return newMap;
        }
    }
}
