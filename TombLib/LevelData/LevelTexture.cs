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
        public const float FootStepSoundGranularity = 64.0f;
        //public const float BumpMappingGranularity = 64.0f;

        public string Path { get; private set; }
        public Exception LoadException { get; private set; }
        public bool Convert512PixelsToDoubleRows { get; private set; }

        private bool _replaceMagentaWithTransparency = true;
        public override bool ReplaceMagentaWithTransparency => _replaceMagentaWithTransparency;

        private TextureFootStepSound[,] _footStepSounds = new TextureFootStepSound[0, 0];
        //private BumpMappingLevel[,] _bumpMappingLevel = new BumpMappingLevel[0, 0];

        public LevelTexture()
        { }

        public LevelTexture(LevelSettings settings, string path, bool convert512PixelsToDoubleRows = false, bool replaceWithTransparency = true)
        {
            Convert512PixelsToDoubleRows = convert512PixelsToDoubleRows;
            _replaceMagentaWithTransparency = replaceWithTransparency;
            SetPath(settings, path);
        }

        public override Texture Clone()
        {
            return new LevelTexture
            {
                UniqueID = UniqueID,
                Image = Image,
                Path = Path,
                LoadException = LoadException,
                Convert512PixelsToDoubleRows = Convert512PixelsToDoubleRows,
                _replaceMagentaWithTransparency = _replaceMagentaWithTransparency,
                _footStepSounds = _footStepSounds,
                //_bumpMappingLevel = _bumpMappingLevel
            };
        }

        public void Assign(LevelTexture other)
        {
            Image = other.Image;
            Path = other.Path;
            LoadException = other.LoadException;
            Convert512PixelsToDoubleRows = other.Convert512PixelsToDoubleRows;
            _replaceMagentaWithTransparency = other._replaceMagentaWithTransparency;
            _footStepSounds = other._footStepSounds;
            //_bumpMappingLevel = other._bumpMappingLevel;
        }

        public bool Equals(LevelTexture other)
        {
            if (!Path.Equals(other.Path, StringComparison.InvariantCultureIgnoreCase))
                return false;
            if (Convert512PixelsToDoubleRows != other.Convert512PixelsToDoubleRows)
                return false;
            if (ReplaceMagentaWithTransparency != other.ReplaceMagentaWithTransparency)
                return false;
            if (FootStepSoundWidth != other.FootStepSoundWidth || FootStepSoundHeight != other.FootStepSoundHeight)
                return false;
            for (int x = 0; x < FootStepSoundWidth; ++x)
                for (int y = 0; y < FootStepSoundHeight; ++y)
                    if (_footStepSounds[x, y] != other._footStepSounds[x, y])
                        return false;
            // Bump mapping!!!!!!!!!!!!!!!!!
            return true;
        }

        public override bool Equals(object other) => other is LevelTexture && Equals((LevelTexture)other);

        public override int GetHashCode() => Path == null ? 0 : Path.GetHashCode();

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
            return Filename + " (at " + Path + ")";
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
                ResizeFootStepSounds(
                    (int)Math.Ceiling(Image.Width / FootStepSoundGranularity),
                    (int)Math.Ceiling(Image.Height / FootStepSoundGranularity));

                // Resize bump maps
                // ResizeBumpMappingInfos(
                //    (int)Math.Ceiling(Image.Width / BumpMappingGranularity),
                //    (int)Math.Ceiling(Image.Height / BumpMappingGranularity));
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

        public void SetReplaceMagentaWithTransparency(LevelSettings settings, bool value)
        {
            if (_replaceMagentaWithTransparency == value)
                return;
            _replaceMagentaWithTransparency = value;
            Reload(settings);
        }

        public TextureFootStepSound GetFootStepSound(int x, int y)
        {
            return _footStepSounds[x, y];
        }

        public void SetFootStepSound(int x, int y, TextureFootStepSound value)
        {
            _footStepSounds[x, y] = value;
        }

        public TextureFootStepSound? GetTextureSoundFromTexCoord(Vector2 coord)
        {
            coord /= FootStepSoundGranularity;
            if (!(coord.X >= 0 && coord.Y >= 0 && coord.X < FootStepSoundWidth && coord.Y < FootStepSoundHeight))
                return null;
            return _footStepSounds[(int)coord.X, (int)coord.Y];
        }

        public int FootStepSoundWidth => _footStepSounds.GetLength(0);
        public int FootStepSoundHeight => _footStepSounds.GetLength(1);

        public void ResizeFootStepSounds(int width, int height)
        {
            int requiredTextureSoundWidth = Math.Max(FootStepSoundWidth, width);
            int requiredTextureSoundHeight = Math.Max(FootStepSoundHeight, height);
            if (FootStepSoundWidth < requiredTextureSoundWidth ||
                FootStepSoundHeight < requiredTextureSoundHeight)
            {
                var newTextureSounds = new TextureFootStepSound[requiredTextureSoundWidth, requiredTextureSoundHeight];
                for (int y = 0; y < requiredTextureSoundHeight; ++y)
                    for (int x = 0; x < requiredTextureSoundWidth; ++x)
                        newTextureSounds[x, y] = TextureFootStepSound.Stone;
                for (int y = 0; y < FootStepSoundHeight; ++y)
                    for (int x = 0; x < FootStepSoundWidth; ++x)
                        newTextureSounds[x, y] = _footStepSounds[x, y];
                _footStepSounds = newTextureSounds;
            }
        }

        /*public BumpMappingLevel GetBumpMapLevel(int x, int y)
        {
            return _bumpMappingLevel[x, y];
        }

        public void SetBumpMappingLevel(int x, int y, BumpMappingLevel info)
        {
            _bumpMappingLevel[x, y] = info;
        }

        public BumpMappingLevel? GetBumpMappingLevelFromTexCoord(Vector2 coord)
        {
            coord /= BumpMappingGranularity;
            if (!(coord.X >= 0 && coord.Y >= 0 && coord.X < BumpMappingWidth && coord.Y < BumpMappingHeight))
                return null;
            return _bumpMappingLevel[(int)coord.X, (int)coord.Y];
        }

        public int BumpMappingWidth => _bumpMappingLevel.GetLength(0);
        public int BumpMappingHeight => _bumpMappingLevel.GetLength(1);

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
        }*/

    }
}
