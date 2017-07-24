using System;
using System.IO;
using TombEditor.Geometry;
using TombLib.IO;
using TombLib.Wad;

namespace TombEditor.Compilers
{
    public partial class LevelCompilerTR4
    {
        private byte[] _textures16;

        private void PrepareTextures()
        {
            ReportProgress(10, "Building final texture map");

            NumRoomTextureTiles = (ushort)_numRoomTexturePages;
            NumObjectTextureTiles = (ushort)(_numobjectTexturePages + _numSpriteTexturePages);

            byte[] uncTexture32 = new byte[_roomTexturePages.Length + _objectTexturePages.Length + _spriteTexturePages.Length];

            Array.Copy(_roomTexturePages, 0, uncTexture32, 0, _roomTexturePages.Length);
            Array.Copy(_objectTexturePages, 0, uncTexture32, _roomTexturePages.Length, _objectTexturePages.Length);
            Array.Copy(_spriteTexturePages, 0, uncTexture32, _roomTexturePages.Length + _objectTexturePages.Length, _spriteTexturePages.Length);

            ReportProgress(80, "Packing 32 bit textures to 16 bit");
            byte[] uncTexture16 = PackTextureMap32To16bit(uncTexture32, 256, (_numRoomTexturePages + _numobjectTexturePages + _numSpriteTexturePages) * 256);

            ReportProgress(80, "Compressing 32 bit textures");
            Texture32 = Utils.CompressDataZLIB(uncTexture32);
            Texture32UncompressedSize = (uint)uncTexture32.Length;
            Texture32CompressedSize = (uint)Texture32.Length;

            _textures16 = uncTexture16;

            ReportProgress(80, "Compressing 16 bit textures");
            Texture16 = Utils.CompressDataZLIB(uncTexture16);
            Texture16UncompressedSize = (uint)uncTexture16.Length;
            Texture16CompressedSize = (uint)Texture16.Length;
        }

        public void BuildWadTexturePages()
        {
            ReportProgress(7, "Building WAD textures pages");

            TR4Wad wad = _editor.Level.Wad.OriginalWad;
            int x;
            int y;

            _objectTexturePages = new byte[256 * 256 * 4 * wad.NumTexturePages];
            _numobjectTexturePages = wad.NumTexturePages;

            for (y = 0; y < wad.NumTexturePages * 256; y++)
            {
                for (x = 0; x < 256; x++)
                {
                    byte r = wad.TexturePages[y, 3 * x + 0];
                    byte g = wad.TexturePages[y, 3 * x + 1];
                    byte b = wad.TexturePages[y, 3 * x + 2];

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
            LevelTexture txt = _level.TextureSamples[texture];

            for (int i = 0; i < _level.TextureSounds.Count; i++)
            {
                TextureSound txtSound = _level.TextureSounds[i];
                if (txt.X >= txtSound.X && txt.Y >= txtSound.Y && txt.X <= txtSound.X + 64 && txt.Y <= txtSound.Y + 64 &&
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

                BinaryReaderEx reader = new BinaryReaderEx(File.OpenRead("Graphics\\Common\\font.pc"));
                byte[] uncMiscTexture = new byte[256 * 256 * 4 * 2];

                byte[] buffer = new byte[256 * 256 * 4];

                reader.ReadBlockArray(out buffer, 256 * 256 * 4);
                reader.Close();

                Array.Copy(buffer, 0, uncMiscTexture, 0, 256 * 256 * 4);

                buffer = new byte[256 * 256 * 4];

                // If exists a sky with the same name of WAD, use it, otherwise take the default sky
                string skyFileName;
                if (File.Exists(_editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName + ".raw"))
                    skyFileName = _editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName + ".raw";
                else
                    skyFileName = "Graphics\\Common\\pcsky.raw";

                ReportProgress(18, "Reading sky texture: " + skyFileName);


                reader = new BinaryReaderEx(File.OpenRead(skyFileName));

                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        byte r = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte b = reader.ReadByte();

                        buffer[y * 1024 + 4 * x] = b;
                        buffer[y * 1024 + 4 * x + 1] = g;
                        buffer[y * 1024 + 4 * x + 2] = r;
                        buffer[y * 1024 + 4 * x + 3] = 255;

                    }
                }

                Array.Copy(buffer, 0, uncMiscTexture, 256 * 256 * 4, 256 * 256 * 4);

                ReportProgress(80, "Compressing font & sky textures");

                MiscTexture = Utils.CompressDataZLIB(uncMiscTexture);
                MiscTextureUncompressedSize = (uint)uncMiscTexture.Length;
                MiscTextureCompressedSize = (uint)MiscTexture.Length;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        
        private byte[] PackTextureMap32To16bit(byte[] map, int width, int height)
        {
            byte[] newMap = new byte[width * height * 2];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ushort a1 = map[y * 256 * 4 + x * 4 + 3];
                    ushort r1 = map[y * 256 * 4 + x * 4 + 2];
                    ushort g1 = map[y * 256 * 4 + x * 4 + 1];
                    ushort b1 = map[y * 256 * 4 + x * 4 + 0];

                    ushort r = (ushort)(r1 / 8);
                    ushort g = (ushort)(g1 / 8);
                    ushort b = (ushort)(b1 / 8);
                    ushort a = 0x8000;

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

                    if (r1 < 8) r = 0;
                    if (g1 < 8) g = 0;
                    if (b1 < 8) b = 0;

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
