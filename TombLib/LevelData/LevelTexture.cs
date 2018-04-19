using NLog;
using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public class LevelTexture : Texture, IEquatable<LevelTexture>
    {
        public static IReadOnlyList<FileFormat> FileExtensions => ImageC.FromFileFileExtensions;

        public class UniqueIDType { }
        public UniqueIDType UniqueID { get; private set; } = new UniqueIDType();

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public const float TextureSoundGranularity = 64.0f;
        public const float BumpMappingGranularity = 64.0f;

        public string Path { get; private set; }
        public Exception LoadException { get; private set; }
        public bool Convert512PixelsToDoubleRows { get; private set; }

        private bool _replaceMagentaWithTransparency = true;
        public override bool ReplaceMagentaWithTransparency => _replaceMagentaWithTransparency;

        private TextureSound[,] _textureSounds = new TextureSound[0, 0];
        private BumpMappingLevel[,] _bumpMappingLevel = new BumpMappingLevel[0, 0];

        public LevelTexture()
        { }

        public LevelTexture(LevelSettings settings, string path, bool convert512PixelsToDoubleRows = false, bool replaceWithTransparency = true)
        {
            Convert512PixelsToDoubleRows = convert512PixelsToDoubleRows;
            _replaceMagentaWithTransparency = replaceWithTransparency;
            SetPath(settings, path);
        }

        public void Reload(LevelSettings settings)
        {
            LoadException = null;
            if (string.IsNullOrEmpty(Path))
            {
                Image = UnloadedPlaceholder;
                return;
            }

            // Load image
            try
            {
                ImageC image = ImageC.FromFile(settings.MakeAbsolute(Path));

                if (Convert512PixelsToDoubleRows && image.Width == 512)
                {
                    ImageC newImage = ImageC.CreateNew(256, image.Height * 2);
                    for (int oldY = 0; oldY < image.Height; oldY += 64)
                    {
                        newImage.CopyFrom(0, oldY * 2, image, 0, oldY, 256, 64);
                        newImage.CopyFrom(0, oldY * 2 + 64, image, 256, oldY, 256, 64);
                    }
                    image = newImage;
                }

                if (ReplaceMagentaWithTransparency)
                    image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                Image = image;

                // Resize sound array
                ResizeTextureSounds(
                    (int)Math.Ceiling(Image.Width / TextureSoundGranularity),
                    (int)Math.Ceiling(Image.Height / TextureSoundGranularity));

                // Resize bump maps
                ResizeBumpMappingInfos(
                    (int)Math.Ceiling(Image.Width / BumpMappingGranularity),
                    (int)Math.Ceiling(Image.Height / BumpMappingGranularity));
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to load texture '" + Path + "'.");
                Image = UnloadedPlaceholder;
                LoadException = exc;
            }
        }

        public void SetPath(LevelSettings settings, string path)
        {
            Path = path;
            Reload(settings);
        }

        public void SetConvert512PixelsToDoubleRows(LevelSettings settings, bool value)
        {
            if (Convert512PixelsToDoubleRows == value)
                return;
            Convert512PixelsToDoubleRows = value;
            if (Image.Width == 512)
                Reload(settings);
        }

        public void SetReplaceWithTransparency(LevelSettings settings, bool value)
        {
            if (_replaceMagentaWithTransparency == value)
                return;
            _replaceMagentaWithTransparency = value;
            Reload(settings);
        }

        public override Texture Clone()
        {
            return new LevelTexture
            {
                UniqueID = UniqueID,
                Image = Image,
                Path = Path,
                LoadException = LoadException,
                _replaceMagentaWithTransparency = _replaceMagentaWithTransparency
            };
        }

        public TextureSound GetTextureSound(int x, int y)
        {
            return _textureSounds[x, y];
        }

        public BumpMappingLevel GetBumpMapLevel(int x, int y)
        {
            return _bumpMappingLevel[x, y];
        }

        public void SetTextureSound(int x, int y, TextureSound value)
        {
            _textureSounds[x, y] = value;
        }

        public void SetBumpMappingLevel(int x, int y, BumpMappingLevel info)
        {
            _bumpMappingLevel[x, y] = info;
        }

        public TextureSound? GetTextureSoundFromTexCoord(Vector2 coord)
        {
            coord /= TextureSoundGranularity;
            if (!(coord.X >= 0 && coord.Y >= 0 && coord.X < TextureSoundWidth && coord.Y < TextureSoundHeight))
                return null;
            return _textureSounds[(int)coord.X, (int)coord.Y];
        }

        public BumpMappingLevel? GetBumpMappingLevelFromTexCoord(Vector2 coord)
        {
            coord /= BumpMappingGranularity;
            if (!(coord.X >= 0 && coord.Y >= 0 && coord.X < BumpMappingWidth && coord.Y < BumpMappingHeight))
                return null;
            return _bumpMappingLevel[(int)coord.X, (int)coord.Y];
        }

        public int TextureSoundWidth => _textureSounds.GetLength(0);
        public int TextureSoundHeight => _textureSounds.GetLength(1);

        public int BumpMappingWidth => _bumpMappingLevel.GetLength(0);
        public int BumpMappingHeight => _bumpMappingLevel.GetLength(1);

        public void ResizeTextureSounds(int width, int height)
        {
            int requiredTextureSoundWidth = Math.Max(TextureSoundWidth, width);
            int requiredTextureSoundHeight = Math.Max(TextureSoundHeight, height);
            if (TextureSoundWidth < requiredTextureSoundWidth ||
                TextureSoundHeight < requiredTextureSoundHeight)
            {
                var newTextureSounds = new TextureSound[requiredTextureSoundWidth, requiredTextureSoundHeight];
                for (int y = 0; y < requiredTextureSoundHeight; ++y)
                    for (int x = 0; x < requiredTextureSoundWidth; ++x)
                        newTextureSounds[x, y] = TextureSound.Stone;
                for (int y = 0; y < TextureSoundHeight; ++y)
                    for (int x = 0; x < TextureSoundWidth; ++x)
                        newTextureSounds[x, y] = _textureSounds[x, y];
                _textureSounds = newTextureSounds;
            }
        }

        public void ResizeBumpMappingInfos(int width, int height)
        {
            int requiredBumpMappingWidth = Math.Max(BumpMappingWidth, width);
            int requiredBumpMappingHeight = Math.Max(BumpMappingHeight, height);
            if (BumpMappingWidth < requiredBumpMappingWidth ||
                BumpMappingHeight < requiredBumpMappingHeight)
            {
                var newBumpMappingInfos = new BumpMappingLevel[requiredBumpMappingWidth, requiredBumpMappingHeight];
                for (int y = 0; y < requiredBumpMappingHeight; ++y)
                    for (int x = 0; x < requiredBumpMappingWidth; ++x)
                        newBumpMappingInfos[x, y] = BumpMappingLevel.None;
                for (int y = 0; y < BumpMappingHeight; ++y)
                    for (int x = 0; x < BumpMappingWidth; ++x)
                        newBumpMappingInfos[x, y] = _bumpMappingLevel[x, y];
                _bumpMappingLevel = newBumpMappingInfos;
            }
        }

        public bool Equals(LevelTexture other) => base.Equals(other);

        public void Assign(LevelTexture other)
        {
            Image = other.Image;
            Path = other.Path;
            LoadException = other.LoadException;
            Convert512PixelsToDoubleRows = other.Convert512PixelsToDoubleRows;
            _replaceMagentaWithTransparency = other._replaceMagentaWithTransparency;
        }

        public override string ToString()
        {
            string Filename;
            try
            {
                Filename = System.IO.Path.GetFileNameWithoutExtension(Path);
            }
            catch
            {
                Filename = "<Unnamed>";
            }
            return Filename + " (Level Texture with " + Path + ")";
        }
    }
}
