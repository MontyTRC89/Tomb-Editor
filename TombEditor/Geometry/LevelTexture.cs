using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using TombLib.Utils;

namespace TombEditor.Geometry
{
    public class LevelTexture : Texture
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public string Path { get; private set; }
        public Exception ImageLoadException { get; private set; }
        public bool Convert512PixelsToDoubleRows { get; private set; } = false;

        private bool _replaceMagentaWithTransparency = true;
        public override bool ReplaceMagentaWithTransparency => _replaceMagentaWithTransparency;

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
            ImageLoadException = null;
            if (string.IsNullOrEmpty(Path))
            {
                Image = UnloadedPlaceholder;
                return;
            }

            // Load image
            try
            {
                ImageC image = ImageC.FromFile(settings.MakeAbsolute(Path));

                if ((Convert512PixelsToDoubleRows) && (image.Width == 512))
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
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to load texture '" + Path + "'.");
                Image = UnloadedPlaceholder;
                ImageLoadException = exc;
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
                Image = Image,
                Path = Path,
                ImageLoadException = ImageLoadException,
                _replaceMagentaWithTransparency = _replaceMagentaWithTransparency
            };
        }
    }
}
