using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using TombEditor.Geometry;
using TombLib.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private byte[] _textures16;

        private void PrepareTextures()
        {
            ReportProgress(10, "Building final texture map");

            _numRoomTextureTiles = (ushort)_numRoomTexturePages;
            _numObjectTextureTiles = (ushort)(_numobjectTexturePages + _numSpriteTexturePages);

            var uncTexture32 =
                new byte[_roomTexturePages.Length + _objectTexturePages.Length + _spriteTexturePages.Length];

            Array.Copy(_roomTexturePages, 0, uncTexture32, 0, _roomTexturePages.Length);
            Array.Copy(_objectTexturePages, 0, uncTexture32, _roomTexturePages.Length, _objectTexturePages.Length);
            Array.Copy(_spriteTexturePages, 0, uncTexture32, _roomTexturePages.Length + _objectTexturePages.Length,
                _spriteTexturePages.Length);

            ReportProgress(80, "Packing 32 bit textures to 16 bit");
            var uncTexture16 = PackTextureMap32To16Bit(uncTexture32, 256,
                (_numRoomTexturePages + _numobjectTexturePages + _numSpriteTexturePages) * 256);

            ReportProgress(80, "Compressing 32 bit textures");
            _texture32 = Utils.CompressDataZLIB(uncTexture32);
            _texture32UncompressedSize = (uint)uncTexture32.Length;
            _texture32CompressedSize = (uint)_texture32.Length;

            _textures16 = uncTexture16;

            ReportProgress(80, "Compressing 16 bit textures");
            _texture16 = Utils.CompressDataZLIB(uncTexture16);
            _texture16UncompressedSize = (uint)uncTexture16.Length;
            _texture16CompressedSize = (uint)_texture16.Length;
        }

        private void BuildWadTexturePages()
        {
            ReportProgress(7, "Building WAD textures pages");

            var wad = _level.Wad.OriginalWad;

            _objectTexturePages = new byte[256 * 256 * 4 * wad.NumTexturePages];
            _numobjectTexturePages = wad.NumTexturePages;

            for (var y = 0; y < wad.NumTexturePages * 256; y++)
            {
                for (var x = 0; x < 256; x++)
                {
                    var r = wad.TexturePages[y, 3 * x + 0];
                    var g = wad.TexturePages[y, 3 * x + 1];
                    var b = wad.TexturePages[y, 3 * x + 2];

                    if (r == 255 && g == 0 && b == 255)
                    {
                        _objectTexturePages[y * 1024 + 4 * x + 0] = 0;
                        _objectTexturePages[y * 1024 + 4 * x + 1] = 0;
                        _objectTexturePages[y * 1024 + 4 * x + 2] = 0;
                        _objectTexturePages[y * 1024 + 4 * x + 3] = 0;
                    }
                    else
                    {
                        _objectTexturePages[y * 1024 + 4 * x + 0] = b;
                        _objectTexturePages[y * 1024 + 4 * x + 1] = g;
                        _objectTexturePages[y * 1024 + 4 * x + 2] = r;
                        _objectTexturePages[y * 1024 + 4 * x + 3] = 255;
                    }
                }
            }

            ReportProgress(8, "    WAD texture pages: " + wad.NumTexturePages);
        }

        private TextureSounds GetTextureSound(int texture)
        {
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

            return TextureSounds.Stone;
        }

        private bool PrepareFontAndSkyTexture()
        {
            try
            {
                ReportProgress(18, "Building font & sky textures");

                byte[] rawData = new byte[256 * 256 * 4 * 2];
                using (Bitmap image = new Bitmap(256, 512, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        // Read font texture
                        string fontFileName = _level.Settings.FontTextureFileNameAbsoluteOrDefault;
                        ReportProgress(19, "Reading font texture: " + fontFileName);
                        using (Bitmap fontTexture = Geometry.IO.ResourceLoader.LoadRawExtraTexture(fontFileName))
                            g.DrawImageUnscaledAndClipped(fontTexture, new Rectangle(0, 0, 256, 256));

                        // Read sky texture
                        string skyFileName = _level.Settings.SkyTextureFileNameAbsoluteOrDefault;
                        ReportProgress(18, "Reading sky texture: " + skyFileName);
                        using (Bitmap skyTexture = Geometry.IO.ResourceLoader.LoadRawExtraTexture(skyFileName))
                            g.DrawImageUnscaledAndClipped(skyTexture, new Rectangle(0, 256, 256, 256));
                    }

                    // Extract raw texture data
                    BitmapData lockData = image.LockBits(new Rectangle(0, 0, 256, 512), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    System.Runtime.InteropServices.Marshal.Copy(lockData.Scan0, rawData, 0, rawData.Length);
                    image.UnlockBits(lockData);
                }

                ReportProgress(80, "Compressing font & sky textures");
                _miscTexture = Utils.CompressDataZLIB(rawData);
                _miscTextureUncompressedSize = (uint)rawData.Length;
                _miscTextureCompressedSize = (uint)_miscTexture.Length;
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while loading font and sky.");
                return false;
            }

            return true;
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
