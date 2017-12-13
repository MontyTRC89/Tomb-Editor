using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public abstract class BaseGeometryImporter
    {
        public delegate Texture GetTextureDelegate(string absoluteFilePath);

        protected IOGeometrySettings _settings { get; set; }
        private GetTextureDelegate _getTextureCallback { get; }

        public abstract IOModel ImportFromFile(string filename);

        public BaseGeometryImporter(IOGeometrySettings settings, GetTextureDelegate getTextureCallback)
        {
            _settings = settings;
            _getTextureCallback = getTextureCallback;
        }

        protected Texture GetTexture(string baseDirectory, string textureFilePath)
        {
            string absoluteTextureFilePath = Path.Combine(baseDirectory, textureFilePath);
            return _getTextureCallback(absoluteTextureFilePath);
        }

        protected Vector3 ApplyAxesTransforms(Vector3 position)
        {
            if (_settings.SwapXY) { var temp = position.X; position.X = position.Y; position.Y = temp; }
            if (_settings.SwapXZ) { var temp = position.X; position.X = position.Z; position.Z = temp; }
            if (_settings.SwapYZ) { var temp = position.Z; position.Z = position.Y; position.Y = temp; }
            if (_settings.FlipX) { position.X = -position.X; }
            if (_settings.FlipY) { position.Y = -position.Y; }
            if (_settings.FlipZ) { position.Z = -position.Z; }
            position *= _settings.Scale;
            return position;
        }

        protected Vector2 ApplyUVTransform(Vector2 uv, int w, int h)
        {
            if (_settings.FlipUV_V)
            {
                uv.Y = 1.0f - uv.Y;
            }
            if (_settings.WrapUV)
            {
                uv.X -= (uv.X > 1.0f ? (float)Math.Floor(uv.X) : 0.0f);
                uv.Y -= (uv.Y > 1.0f ? (float)Math.Floor(uv.Y) : 0.0f);
            }
            if (_settings.PremultiplyUV)
            {
                uv.X *= w;
                uv.Y *= h;
            }
            return uv;
        }

        protected Vector4 ApplyColorTransform(Vector4 color)
        {
            return color;
        }
    }
}
