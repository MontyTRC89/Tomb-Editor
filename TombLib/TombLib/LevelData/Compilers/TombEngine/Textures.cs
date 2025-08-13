using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using NLog;
using Pfim;
using SharpDX.DXGI;
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
            Sector sector = room.Sectors[x, z];

            TextureFootStep.Type? result0 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(SectorFace.Floor));
            if (result0.HasValue)
                return result0.Value;

            TextureFootStep.Type? result1 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(SectorFace.Floor_Triangle2));
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

        private byte[] GetCompressedTexture(ImageC i, CompressionFormat format)
        {
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

				WriteImageFast(writer, atlas, CompressionFormat.Bc3);
            }

            // Sky texture
            var sky = GetSkyTexture();
			writer.Write(sky.Width);
			writer.Write(sky.Height);
			WriteImageFast(writer, sky, CompressionFormat.Bc3);
		}

        void WriteImageFast(BinaryWriterEx writer, ImageC image, CompressionFormat compressionFormat)
        {
			var stream = writer.BaseStream;
			long lenPos = stream.Position;
			writer.Write(0); // Placeholder
			long startPos = stream.Position;

			if (_level.Settings.CompressTextures)
			{
                writer.Write(GetCompressedTexture(image, compressionFormat));
			}
			else
			{
				image.SavePngToStreamFast(stream);
			}

			long endPos = stream.Position;
			int len = checked((int)(endPos - startPos));

			long cur = endPos;
			stream.Position = lenPos;
			writer.Write(len);
			stream.Position = cur;
		}

        void WriteAtlas(BinaryWriterEx writer, List<TombEngineAtlas> atlasList)
        {
            writer.Write(atlasList.Count);
            foreach (var atlas in atlasList)
			{
				writer.Write(atlas.ColorMap.Width);
				writer.Write(atlas.ColorMap.Height);

				WriteImageFast(writer, atlas.ColorMap, CompressionFormat.Bc3);
                writer.Write(atlas.HasNormalMap);

                if (atlas.HasNormalMap)
                    WriteImageFast(writer, atlas.NormalMap, CompressionFormat.Bc5);
            }
        }
    }
}
