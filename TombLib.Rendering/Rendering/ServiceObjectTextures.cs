using System;
using System.Collections.Generic;
using System.Drawing;
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
                        _serviceTextures.Add(img);
                    }
                }
                return _serviceTextures;
            }
        }

        public static Sprite GetSprite(PositionBasedObjectInstance instance, Camera camera, Size viewportSize, Vector4 color, float zoom = 4.0f)
        {
            ServiceObjectTexture type;

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
            else if (instance is FlybyCameraInstance) type = ServiceObjectTexture.flyby_camera;
            else if (instance is SoundSourceInstance) type = ServiceObjectTexture.sound_source;
            else if (instance is ImportedGeometryInstance) type = ServiceObjectTexture.imp_geo;
            else type = ServiceObjectTexture.unknown;

            var tex = Images[(int)type];
            var width = (int)(tex.Width * zoom);
            var height = (int)(tex.Height * zoom);
            var alignment = new Rectangle2(new Vector2(-width / 2.0f, -height / 2.0f), new Vector2(width / 2.0f, height / 2.0f));

            var heightRatio = ((float)viewportSize.Height / viewportSize.Width) * 1024.0f;
            var distance = Vector3.Distance(instance.Position + instance.Room.WorldPos, camera.GetPosition());
            var scale = 1024.0f / (distance != 0 ? distance : 1.0f);
            var pos = (instance.WorldPositionMatrix * camera.GetViewProjectionMatrix(viewportSize.Width, viewportSize.Height)).TransformPerspectively(new Vector3());
            var screenPos = pos.To2();

            var start = screenPos - scale * new Vector2(alignment.End.X / 1024.0f, alignment.End.Y / heightRatio);
            var end   = screenPos - scale * new Vector2(alignment.Start.X / 1024.0f, alignment.Start.Y / heightRatio);

            return new Sprite()
            {
                Texture = tex,
                PosStart = start,
                PosEnd = end,
                Depth = pos.Z,
                Tint = color
            };
        }
    }
}
