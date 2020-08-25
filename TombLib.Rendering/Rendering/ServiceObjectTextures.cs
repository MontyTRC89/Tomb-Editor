using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;
using System.Reflection;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Rendering
{
    public enum ServiceObjectTexture
    {
        camera,
        flyby_camera,
        imp_geo,
        sink,
        sound_source,
        sprite,
        ghost_block,
        light_effect,
        light_fog,
        light_point,
        light_shadow,
        light_spot,
        light_sun,
        unknown
    }

    public static class ServiceObjectTextures
    {
        private const int   _mipLevels = 4;          // Number of MIP levels, including root one
        private const float _mipStep = 6000.0f;      // Distance steps where mipmaps are switching
        private const float _fadeDistance = 256.0f;  // Distance at which sprite slowly fades out

        private static List<ImageC> _serviceTextures;
        public static List<ImageC> Images
        {
            get
            {
                if (_serviceTextures == null)
                {
                    _serviceTextures = new List<ImageC>();
                    foreach (var name in Enum.GetNames(typeof(ServiceObjectTexture)))
                    {
                        var img = ImageC.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(nameof(TombLib)
                            + "." + nameof(Rendering) + ".ServiceObjectTextures." + name + ".png"));

                        // Add original texture
                        _serviceTextures.Add(img);

                        // Manually add MIP levels.
                        // We can't use automatic DX10 mipmaps because TRTombalized renderer.   

                        var bmp = img.ToBitmap();
                        for (int i = 0; i < _mipLevels; i++)
                        {
                            // Calculate next mip level dimensions
                            var mult = (i + 1) * 2;
                            var destRect = new Rectangle(0, 0, bmp.Width / mult, bmp.Height / mult);
                            var destImage = new Bitmap(bmp.Width / mult, bmp.Height / mult);

                            // Downscale and apply next MIP level
                            destImage.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
                            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
                            {
                                graphics.CompositingMode = CompositingMode.SourceCopy;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graphics.SmoothingMode = SmoothingMode.HighQuality;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.CompositingQuality = CompositingQuality.HighQuality;


                                using (var wrapMode = new ImageAttributes())
                                {
                                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                    graphics.DrawImage(bmp, destRect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, wrapMode);
                                }
                            }

                            // Save next MIP level to image array
                            _serviceTextures.Add(ImageC.FromSystemDrawingImage(destImage));
                        }
                    }
                }
                return _serviceTextures;
            }
        }

        // Gets sprite for specified instance type, if such sprite exists.
        // If no specific sprite for this type exists, returns default image with [?] mark.

        public static Sprite GetSprite(ObjectInstance instance, Camera camera, Size viewportSize, Vector4 color, bool noZ = false, float zoom = 4.5f)
        {
            // We allow sprites only for spatial instances and ghost blocks.
            if (!(instance is ISpatial))
                return null;

            Vector3 absPos;
            Matrix4x4 posMatrix;
            ServiceObjectTexture type;

            if (instance is PositionBasedObjectInstance)
            {
                var obj = (PositionBasedObjectInstance)instance;
                posMatrix = obj.WorldPositionMatrix;
                absPos = obj.Position + obj.Room.WorldPos;
            }
            else if (instance is GhostBlockInstance)
            {
                var obj = (GhostBlockInstance)instance;
                posMatrix = obj.CenterMatrix(true) * Matrix4x4.CreateTranslation(new Vector3(0, 96.0f, 0));
                absPos = obj.Center(true);
            }
            else
                return null; // Unknown non-position-based instance!

            // Determine needed sprite type

            if (instance is LightInstance)
            {
                switch ((instance as LightInstance).Type)
                {
                    case LightType.Effect:
                        type = ServiceObjectTexture.light_effect;
                        break;
                    case LightType.FogBulb:
                        type = ServiceObjectTexture.light_fog;
                        break;
                    case LightType.Point:
                        type = ServiceObjectTexture.light_point;
                        break;
                    case LightType.Shadow:
                        type = ServiceObjectTexture.light_shadow;
                        break;
                    case LightType.Spot:
                        type = ServiceObjectTexture.light_spot;
                        break;
                    case LightType.Sun:
                        type = ServiceObjectTexture.light_sun;
                        break;
                    default:
                        type = ServiceObjectTexture.unknown;
                        break;
                }
            }
            else if (instance is SinkInstance) type = ServiceObjectTexture.sink;
            else if (instance is SpriteInstance) type = ServiceObjectTexture.sprite;
            else if (instance is CameraInstance) type = ServiceObjectTexture.camera;
            else if (instance is GhostBlockInstance) type = ServiceObjectTexture.ghost_block;
            else if (instance is FlybyCameraInstance) type = ServiceObjectTexture.flyby_camera;
            else if (instance is SoundSourceInstance) type = ServiceObjectTexture.sound_source;
            else if (instance is ImportedGeometryInstance) type = ServiceObjectTexture.imp_geo;
            else type = ServiceObjectTexture.unknown;

            // Determine MIP level
            var distance = Vector3.Distance(absPos, camera.GetPosition());
            var index = (int)Math.Min(Math.Max(Math.Round(distance / _mipStep), 0), _mipLevels);

            // Get the sprite and calculate dimensions            
            var refIndex  = (int)type * (_mipLevels + 1);
            var refTex    = Images[refIndex];
            var tex       = Images[refIndex + index];
            var width     = (int)(refTex.Width * zoom);
            var height    = (int)(refTex.Height * zoom);
            var alignment = new Rectangle2(new Vector2(-width / 2.0f, -height / 2.0f), new Vector2(width / 2.0f, height / 2.0f));

            // Calculate screen-space position
            var heightRatio = ((float)viewportSize.Height / viewportSize.Width) * 1024.0f;
            var scale = 1024.0f / (distance != 0 ? distance : 1.0f);
            var pos = (posMatrix * camera.GetViewProjectionMatrix(viewportSize.Width, viewportSize.Height)).TransformPerspectively(new Vector3());
            var screenPos = pos.To2();

            // Do fadeout if sprite is too near
            if (distance < _fadeDistance)
                color *= distance / _fadeDistance;

            // Calculate final viewport coordinates
            var start = screenPos - scale * new Vector2(alignment.End.X / 1024.0f, alignment.End.Y / heightRatio);
            var end   = screenPos - scale * new Vector2(alignment.Start.X / 1024.0f, alignment.Start.Y / heightRatio);

            // Make the sprite
            var result = new Sprite()
            {
                Texture = tex,
                PosStart = start,
                PosEnd = end,
                Tint = color
            };
            
            if (!noZ) result.Depth = pos.Z;  // Assign depth if needed
            if (pos.Z > 1.0f) result = null; // Discard out-of-bounds sprites

            return result;
        }
    }
}
