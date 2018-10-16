using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private void PrepareTextures()
        {
            // It's fine using the old unpadded way for sprites, they are quad
            List<ImageC> spritePages = BuildSprites(_textureInfoManager.NumRoomPages + _textureInfoManager.NumObjectsPages);

            // Get the final number of pages
            int numPages = _textureInfoManager.NumRoomPages + _textureInfoManager.NumObjectsPages +
                spritePages.Count + _textureInfoManager.NumBumpPages * 2;

            ReportProgress(60, "Building final texture map");

            byte[] texture32Data = new byte[numPages * 256 * 256 * 4];
            int totalPages = 0;

            _textureInfoManager.RoomPages.RawCopyTo(texture32Data, totalPages * 256 * 256 * 4);
            totalPages += _textureInfoManager.NumRoomPages;

            _textureInfoManager.ObjectsPages.RawCopyTo(texture32Data, totalPages * 256 * 256 * 4);
            totalPages += _textureInfoManager.NumObjectsPages;

            for (int i = 0; i < spritePages.Count; i++)
                spritePages[i].RawCopyTo(texture32Data, (totalPages + i) * 256 * 256 * 4);
            totalPages += spritePages.Count;

            _textureInfoManager.BumpPages.RawCopyTo(texture32Data, totalPages * 256 * 256 * 4);

            _texture32Data = texture32Data;

            // I need to update the bumped tiles
            _textureInfoManager.UpdateTiles(spritePages.Count);
            
            // DEBUG: dump the texture map
            //var tempImage = ImageC.FromByteArray(texture32Data, 256, numPages * 256);
            //tempImage.Save("H:\\testpack.jpg");

            ReportProgress(70, "    Num room pages: " + _textureInfoManager.NumRoomPages);
            ReportProgress(70, "    Num objects pages: " + _textureInfoManager.NumObjectsPages);
            ReportProgress(70, "    Num bumpmap pages: " + _textureInfoManager.NumBumpPages);
        }

        private TextureFootStepSound? GetTextureSound(bool isTriangle, TextureArea area)
        {
            LevelTexture texture = area.Texture as LevelTexture;
            if (texture == null)
                return null;

            // Top right position for now
            Vector2 topRight = Vector2.Min(Vector2.Min(area.TexCoord0, area.TexCoord1), isTriangle ? area.TexCoord2 : Vector2.Min(area.TexCoord2, area.TexCoord3));
            return texture.GetTextureSoundFromTexCoord(topRight);
        }

        private TextureFootStepSound GetTextureSound(Room room, int x, int z)
        {
            Block sector = room.Blocks[x, z];

            TextureFootStepSound? result0 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(BlockFace.Floor));
            if (result0.HasValue)
                return result0.Value;

            TextureFootStepSound? result1 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(BlockFace.FloorTriangle2));
            if (result1.HasValue)
                return result1.Value;

            return TextureFootStepSound.Stone;
        }

        private Stream PrepareFontAndSkyTexture()
        {
            ReportProgress(96, "Building font & sky textures");

            var image = ImageC.CreateNew(256, _level.Settings.GameVersion == GameVersion.TR5 || _level.Settings.GameVersion == GameVersion.TR5Main ? 768 : 512);
            int toY = 0;

            // Read extra textures
            if (_level.Settings.GameVersion == GameVersion.TR5)
            {
                string extraFileName = _level.Settings.MakeAbsolute(_level.Settings.Tr5ExtraSpritesFilePath);
                if (!string.IsNullOrEmpty(extraFileName) && !File.Exists(extraFileName))
                {
                    _progressReporter.ReportWarn("Specified extra TR5 texture not found, using default.");
                    extraFileName = null;
                }
                else
                    ReportProgress(96, "Reading extra TR5 texture: " + extraFileName);
                image.CopyFrom(0, toY, _level.Settings.LoadTr5ExtraSprites(extraFileName));
                toY += 256;
            }

            // Read font texture
            string fontFileName = _level.Settings.MakeAbsolute(_level.Settings.FontTextureFilePath);
            if (!string.IsNullOrEmpty(fontFileName) && !File.Exists(fontFileName))
            {
                _progressReporter.ReportWarn("Specified font texture not found, using default.");
                fontFileName = null;
            }
            else
                ReportProgress(96, "Reading font texture: " + fontFileName);
            image.CopyFrom(0, toY, _level.Settings.LoadFontTexture(fontFileName));
            toY += 256;


            // Read sky texture
            string skyFileName = _level.Settings.MakeAbsolute(_level.Settings.SkyTextureFilePath);
            if (!string.IsNullOrEmpty(skyFileName) && !File.Exists(skyFileName))
            {
                _progressReporter.ReportWarn("Specified sky texture not found, using default.");
                skyFileName = null;
            }
            else
                ReportProgress(96, "Reading sky texture: " + skyFileName);
            image.CopyFrom(0, toY, _level.Settings.LoadSkyTexture(skyFileName));

            return image.ToRawStream();
        }

        private List<ImageC> BuildSprites(int pagesBeforeSprites)
        {
            ReportProgress(59, "Building sprites");
            var spriteSequences = _level.Settings.WadGetAllSpriteSequences();
            //ReportProgress(9, "Reading " + _level.Wad.OriginalWad.BaseName + ".swd");

            // Add all sprites to the texture packer
            var textureAllocator = new Util.TextureAllocator();
            var spriteTextureIDs = new Dictionary<Hash, int>();
            foreach (var sprite in spriteSequences.Values.SelectMany(sequence => sequence.Sprites))
                if (!spriteTextureIDs.ContainsKey(sprite.Texture.Hash))
                    spriteTextureIDs.Add(sprite.Texture.Hash, textureAllocator.GetOrAllocateTextureID(sprite.Texture));

            // Pack textures
            List<ImageC> texturePages = textureAllocator.PackTextures();

            // Now build data structures
            var tempSequences = new List<tr_sprite_sequence>();
            var tempSprites = new List<tr_sprite_texture>();
            var lastOffset = 0;

            foreach (var oldSequence in spriteSequences.Values)
            {
                var newSequence = new tr_sprite_sequence();
                newSequence.NegativeLength = (short)-oldSequence.Sprites.Count;
                newSequence.ObjectID = (int)oldSequence.Id.TypeId;
                newSequence.Offset = (short)lastOffset;

                foreach (var oldTexture in oldSequence.Sprites)
                {
                    var packInfo = textureAllocator.GetPackInfo(spriteTextureIDs[oldTexture.Texture.Hash]);
                    var newTexture = new tr_sprite_texture();

                    if (_level.Settings.GameVersion <= GameVersion.TR3)
                    {
                        newTexture.X = (byte)packInfo.Pos.X;
                        newTexture.Y = (byte)packInfo.Pos.Y;
                        newTexture.Width = (ushort)((oldTexture.Texture.Image.Width - 1) * 256 + 255);
                        newTexture.Height = (ushort)((oldTexture.Texture.Image.Height - 1) * 256 + 255);
                        newTexture.TopSide = 0;
                        newTexture.LeftSide = 0;
                        newTexture.RightSide = 0;
                        newTexture.BottomSide = 0;
                    }
                    else
                    {
                        newTexture.TopSide = (short)packInfo.Pos.Y;
                        newTexture.LeftSide = (short)packInfo.Pos.X;
                        newTexture.X = (byte)newTexture.LeftSide;
                        newTexture.Y = (byte)newTexture.TopSide;
                        newTexture.Width = (ushort)((oldTexture.Texture.Image.Width - 1) * 256);
                        newTexture.Height = (ushort)((oldTexture.Texture.Image.Height - 1) * 256);
                        newTexture.RightSide = (short)(newTexture.LeftSide + oldTexture.Texture.Image.Width);
                        newTexture.BottomSide = (short)(newTexture.TopSide + oldTexture.Texture.Image.Height);
                    }
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

        private static byte[] PackTextureMap32To16Bit(byte[] textureData)
        {
            int pixelCount = textureData.Length / 4;
            byte[] newTextureData = new byte[pixelCount * 2];
            for (int i = 0; i < pixelCount; i++)
            {
                ushort a1 = textureData[i * 4 + 3];
                ushort r1 = textureData[i * 4 + 2];
                ushort g1 = textureData[i * 4 + 1];
                ushort b1 = textureData[i * 4 + 0];

                ushort r = (ushort)((r1 + 4) >> 3);
                ushort g = (ushort)((g1 + 4) >> 3);
                ushort b = (ushort)((b1 + 4) >> 3);

                ushort tmp = a1 > 127 ? (ushort)0x8000 : (ushort)0;
                tmp |= (ushort)(r << 10);
                tmp |= (ushort)(g << 5);
                tmp |= b;

                newTextureData[i * 2] = (byte)(tmp & 0xff);
                newTextureData[i * 2 + 1] = (byte)((tmp & 0xff00) >> 8);
            }
            return newTextureData;
        }
    }
}
