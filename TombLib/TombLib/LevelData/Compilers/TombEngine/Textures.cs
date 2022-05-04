using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

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

            TextureFootStep.Type? result1 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(BlockFace.FloorTriangle2));
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
            var spriteAllocator = new Util.SpriteAllocator();
            var spriteTextureIDs = new Dictionary<Hash, int>();
            foreach (var sprite in spriteSequences.Values.SelectMany(sequence => sequence.Sprites))
                if (!spriteTextureIDs.ContainsKey(sprite.Texture.Hash))
                    spriteTextureIDs.Add(sprite.Texture.Hash, spriteAllocator.GetOrAllocateTextureID(sprite.Texture));

            // Pack textures
            _spritesTexturesPages = new List<ImageC>(); //spriteAllocator.PackTextures();

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

                    /*var packInfo = spriteAllocator.GetPackInfo(id);
                 
                    float x = packInfo.Pos.X / (float)_spritesTexturesPages[packInfo.OutputTextureID].Width;
                    float y = packInfo.Pos.Y / (float)_spritesTexturesPages[packInfo.OutputTextureID].Height;
                    float w = (sprite.Texture.Image.Width - 1) / (float)_spritesTexturesPages[packInfo.OutputTextureID].Width;
                    float h = (sprite.Texture.Image.Height - 1) / (float)_spritesTexturesPages[packInfo.OutputTextureID].Height;*/

                    var newTexture = new TombEngineSpriteTexture();

                    newTexture.Tile = _spritesTexturesPages.Count; // packInfo.OutputTextureID;
                    newTexture.X1 = 0.0f;
                    newTexture.Y1 = 0.0f;
                    newTexture.X2 = 1.0f;
                    newTexture.Y2 = 0.0f;
                    newTexture.X3 = 1.0f;
                    newTexture.Y3 = 1.0f;
                    newTexture.X4 = 0.0f;
                    newTexture.Y4 = 1.0f;

                    _spritesTexturesPages.Add(sprite.Texture.Image);
                    tempSprites.Add(newTexture);
                    //sprite.Texture.Image.Save("F:\\sprite" + newTexture.Tile + ".png");
                }

                tempSequences.Add(newSequence);
                lastOffset += sequence.Sprites.Count;
            }

            _spriteSequences = tempSequences;
            _spriteTextures = tempSprites;
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
    }
}
