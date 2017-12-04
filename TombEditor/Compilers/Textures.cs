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
        private const int texturePageSize = (256 * 256 * 4);

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private void PrepareTextures()
        {
            List<ImageC>[] packedTextures = _objectTextureManager.PackTextures(_progressReporter);
            List<ImageC> spritePages = BuildSprites(packedTextures[0].Count);

            _objectTextureManager.NumNonBumpedTiles = packedTextures[0].Count + spritePages.Count;
            _objectTextureManager.NumBumpedTilesLevel1 = packedTextures[1].Count;
            _objectTextureManager.NumBumpedTilesLevel2 = packedTextures[2].Count;

            ReportProgress(10, "Building final texture map");

            int offset = 0;

            byte[] texture32Data = new byte[(spritePages.Count + packedTextures[0].Count + ((packedTextures[1].Count + packedTextures[2].Count) * 2)) * texturePageSize];

            for (int i = 0; i < packedTextures[0].Count; ++i)
            {
                packedTextures[0][i].RawCopyTo(texture32Data, offset);
                offset += texturePageSize;
            }

            for (int i = 0; i < spritePages.Count; ++i)
            {
                spritePages[i].RawCopyTo(texture32Data, offset);
                offset += texturePageSize;
            }

            // Copy bump level 1 and 2 tiles
            for (int p = 0; p < 2; p++)
                for (int i = 0; i < packedTextures[p + 1].Count; ++i)
                {
                    packedTextures[p + 1][i].RawCopyTo(texture32Data, offset);
                    offset += texturePageSize;
                }

            // Apply embossing to each page (BROKEN! SHOULD BE APPLIED TO EACH TEXTURE AREA INDEPENDENTLY!)
            for (int p = 0; p < 2; p++)
                for (int i = 0; i < packedTextures[p + 1].Count; ++i)
                {
                    int Xstride = packedTextures[p + 1][i].Width / 8;
                    int Ystride = packedTextures[p + 1][i].Height / 8;

                    for (int x = 0; x < packedTextures[p + 1][i].Width; x += Xstride)
                        for (int y = 0; y < packedTextures[p + 1][i].Height; y += Ystride)
                            packedTextures[p + 1][i].Emboss(x, y, Xstride, Ystride, -2, p+2);

                    packedTextures[p + 1][i].RawCopyTo(texture32Data, offset);
                    offset += texturePageSize;
                }

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

            // Add all sprites to the texture packer
            var textureAllocator = new Util.TextureAllocator();
            var spriteTextureIDs = new Dictionary<Hash, int>();
            foreach (var sprite in _level.Wad.SpriteTextures)
                spriteTextureIDs.Add(sprite.Key, textureAllocator.GetOrAllocateTextureID(sprite.Value));

            // Pack textures
            List<ImageC> texturePages = textureAllocator.PackTextures();

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

                foreach (var oldTexture in oldSequence.Sprites)
                {
                    var packInfo = textureAllocator.GetPackInfo(spriteTextureIDs[oldTexture.Hash]);
                    var newTexture = new tr_sprite_texture();

                    newTexture.TopSide = (short)packInfo.Pos.Y;
                    newTexture.LeftSide = (short)packInfo.Pos.X;
                    newTexture.X = (byte)newTexture.LeftSide;
                    newTexture.Y = (byte)newTexture.TopSide;
                    newTexture.Width = (ushort)((oldTexture.Width - 1) * 256);
                    newTexture.Height = (ushort)((oldTexture.Height - 1) * 256);
                    newTexture.RightSide = (short)(newTexture.LeftSide + (oldTexture.Width));
                    newTexture.BottomSide = (short)(newTexture.TopSide + (oldTexture.Height));
                    newTexture.Tile = (ushort)(pagesBeforeSprites + packInfo.OutputTextureID);

                    tempSprites.Add(newTexture);
                }

                tempSequences.Add(newSequence);
                lastOffset += oldSequence.Sprites.Count;
            }

            _spriteSequences = tempSequences;
            _spriteTextures = tempSprites;

            return texturePages;
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
