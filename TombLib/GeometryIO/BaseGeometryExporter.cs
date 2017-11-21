using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public abstract class BaseGeometryExporter
    {
        public delegate string GetTextureDelegate(Texture texture);

        protected IOGeometrySettings _settings { get; }
        private GetTextureDelegate _getTexturePathCallback { get; }

        public abstract bool ExportToFile(IOModel model, string filename);

        public BaseGeometryExporter(IOGeometrySettings settings, GetTextureDelegate getTexturePathCallback)
        {
            _settings = settings;
            _getTexturePathCallback = getTexturePathCallback;
        }

        protected string GetTexturePath(string baseDirectory, Texture texture)
        {
            string texturePath = _getTexturePathCallback(texture);
            texturePath = PathC.GetRelativePath(baseDirectory, texturePath);
            return texturePath;
        }

        protected Vector3 ApplyAxesTransforms(Vector3 position)
        {
            if (_settings.SwapXY) { var temp = position.X; position.X = position.Y; position.Y = temp; }
            if (_settings.SwapXZ) { var temp = position.X; position.X = position.Z; position.Z = temp; }
            if (_settings.SwapYZ) { var temp = position.Z; position.Z = position.Y; position.Y = temp; }
            if (_settings.FlipX) { position.X = -position.X; }
            if (_settings.FlipY) { position.Y = -position.Y; }
            if (_settings.FlipZ) { position.Z = -position.Z; }
            position /= _settings.Scale;
            return position;
        }

        protected Vector2 RoundUV(Vector2 uv)
        {
            if ((int)(uv.X - 0.5f) % 16 == 0)
                uv.X -= 0.5f;
            else
                uv.X += 0.5f;

            if ((int)(uv.Y - 0.5f) % 16 == 0)
                uv.Y -= 0.5f;
            else
                uv.Y += 0.5f;

            return uv;
        }

        protected Vector2 ApplyUVTransform(Vector2 uv, int w, int h)
        {
            if (_settings.PremultiplyUV)
            {
                uv.X /= w;
                uv.Y /= h;
            }
            if (_settings.FlipUV_V)
            {
                uv.Y = 1.0f - uv.Y;
            }
            return uv;
        }

        protected Vector4 ApplyColorTransform(Vector4 color)
        {
            return color;
        }
    }
}
