using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using TombEditor.Geometry;
using TombLib.IO;
using System.Drawing;
using System.Drawing.Imaging;
using TombLib.Utils;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private byte[] _textures16;

        private void PrepareTextures()
        {
            List<ImageC> packedTextures = _objectTextureManager.PackTextures(_progressReporter);
            List<ImageC> spritePages = BuildSprites(packedTextures.Count);

            ReportProgress(10, "Building final texture map");

            // The room texture tile count currently also currently contains the wad textures
            // But lets not bother with those fielsd too much since they only matter when bump maps are used and we don't use them.
            _numRoomTextureTiles = (ushort)packedTextures.Count;
            _numObjectTextureTiles = (ushort)(spritePages.Count);
            
            byte[] uncTexture32 = new byte[(spritePages.Count + packedTextures.Count) * (256 * 256 * 4)];

            for (int i = 0; i < packedTextures.Count; ++i)
                packedTextures[i].RawCopyTo(uncTexture32, i * (256 * 256 * 4));
            for (int i = 0; i < spritePages.Count; ++i)
                spritePages[i].RawCopyTo(uncTexture32, (packedTextures.Count + i) * (256 * 256 * 4));

            ReportProgress(80, "Packing 32 bit textures to 16 bit");
            byte[] uncTexture16 = PackTextureMap32To16Bit(uncTexture32, 256, uncTexture32.GetLength(0) / (256 * 4));

            ReportProgress(80, "Compressing 32 bit textures");
            _texture32 = ZLib.CompressData(uncTexture32);
            _texture32UncompressedSize = (uint)uncTexture32.Length;
            _texture32CompressedSize = (uint)_texture32.Length;

            _textures16 = uncTexture16;

            ReportProgress(80, "Compressing 16 bit textures");
            _texture16 = ZLib.CompressData(uncTexture16);
            _texture16UncompressedSize = (uint)uncTexture16.Length;
            _texture16CompressedSize = (uint)_texture16.Length;
        }

        private TextureSound GetTextureSound(Room room, int x, int z)
        {/*
            var txt = _level.TextureSamples[texture];

            foreach (var txtSound in _level.TextureSounds)
            {
                if (txt.X >= txtSound.X && txt.Y >= txtSound.Y && txt.X < txtSound.X + 64 &&
                    txt.Y < txtSound.Y + 64 &&
                    txt.Page == txtSound.Page)
                {
                    return txtSound.Sound;
                }
            }
            */
            return TextureSound.Stone;
        }

        private bool PrepareFontAndSkyTexture()
        {
            try
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
                
                ReportProgress(80, "Compressing font & sky textures");
                var rawDataStream = image.ToRawStream();
                _miscTexture = ZLib.CompressData(image.ToRawStream());
                _miscTextureUncompressedSize = (uint)rawDataStream.Length;
                _miscTextureCompressedSize = (uint)_miscTexture.Length;
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while loading font and sky.");
                return false;
            }

            return true;
        }

        private List<ImageC> BuildSprites(int pagesBeforeSprites)
        {
            ReportProgress(9, "Building sprites");
            ReportProgress(9, "Reading " + _level.Wad.OriginalWad.BaseName + ".swd");

            List<ImageC> texturePages = new List<ImageC>();

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
