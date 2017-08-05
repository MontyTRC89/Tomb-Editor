using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using TombEditor.Geometry;
using TombLib.IO;

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

            var wad = _editor.Level.Wad.OriginalWad;

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
                if (txt.X >= txtSound.X && txt.Y >= txtSound.Y && txt.X <= txtSound.X + 64 &&
                    txt.Y <= txtSound.Y + 64 &&
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

                byte[] buffer;
                byte[] uncMiscTexture;
                using (var reader = new BinaryReaderEx(File.OpenRead("Graphics\\Common\\font.pc")))
                {
                    uncMiscTexture = new byte[256 * 256 * 4 * 2];
                    reader.ReadBlockArray(out buffer, 256 * 256 * 4);
                }

                Array.Copy(buffer, 0, uncMiscTexture, 0, 256 * 256 * 4);

                buffer = new byte[256 * 256 * 4];

                // If exists a sky with the same name of WAD, use it, otherwise take the default sky
                string skyFileName;
                if (File.Exists(_editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName +
                                ".raw"))
                    skyFileName = _editor.Level.Wad.OriginalWad.BasePath + "\\" +
                                  _editor.Level.Wad.OriginalWad.BaseName + ".raw";
                else
                    skyFileName = "Graphics\\Common\\pcsky.raw";

                ReportProgress(18, "Reading sky texture: " + skyFileName);


                using (var reader = new BinaryReaderEx(File.OpenRead(skyFileName)))
                {
                    for (var y = 0; y < 256; y++)
                    {
                        for (var x = 0; x < 256; x++)
                        {
                            var r = reader.ReadByte();
                            var g = reader.ReadByte();
                            var b = reader.ReadByte();

                            buffer[y * 1024 + 4 * x] = b;
                            buffer[y * 1024 + 4 * x + 1] = g;
                            buffer[y * 1024 + 4 * x + 2] = r;
                            buffer[y * 1024 + 4 * x + 3] = 255;
                        }
                    }
                }

                Array.Copy(buffer, 0, uncMiscTexture, 256 * 256 * 4, 256 * 256 * 4);

                ReportProgress(80, "Compressing font & sky textures");

                _miscTexture = Utils.CompressDataZLIB(uncMiscTexture);
                _miscTextureUncompressedSize = (uint)uncMiscTexture.Length;
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
