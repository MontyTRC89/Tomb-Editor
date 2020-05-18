﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
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
            var texturePath = _getTexturePathCallback(texture);
            var relativeTexturePath = PathC.GetRelativePath(baseDirectory, texturePath);
            if (relativeTexturePath == null || relativeTexturePath == "") return texturePath;
            return relativeTexturePath;
        }

        public static BaseGeometryExporter CreateForFile(string filename, IOGeometrySettings settings, GetTextureDelegate getTexturePathCallback)
        {
            if (filename.EndsWith(".mqo", StringComparison.InvariantCultureIgnoreCase))
                return new Exporters.MetasequoiaExporter(settings, getTexturePathCallback);
            else if (filename.EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase))
                return new Exporters.ObjExporter(settings, getTexturePathCallback);
            else if (filename.EndsWith(".gltf", StringComparison.InvariantCultureIgnoreCase))
                return new Exporters.Assimp(settings, Exporters.Assimp.Exporter.GLTF2, getTexturePathCallback);
            else if (filename.EndsWith(".x3d", StringComparison.InvariantCultureIgnoreCase))
                return new Exporters.Assimp(settings, Exporters.Assimp.Exporter.X3d, getTexturePathCallback);
            else if (filename.EndsWith(".dae", StringComparison.InvariantCultureIgnoreCase))
                return new Exporters.Assimp(settings, Exporters.Assimp.Exporter.Collada, getTexturePathCallback);
            else
                throw new NotSupportedException("Unsupported file extension '" + Path.GetExtension(filename) + "'");
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

        protected Vector2 ApplyUVTransform(Vector2 uv, int w = 1, int h = 1)
        {
            uv.X = (float)Math.Round(uv.X);
            uv.Y = (float)Math.Round(uv.Y);

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
            // Clamp each value separately to prevent overflows which are possible because TE doesn't internally
            // clamp its vertex lighting to 2.0f.

            color.X = MathC.Clamp(color.X, 0.0f, 2.0f);
            color.Y = MathC.Clamp(color.Y, 0.0f, 2.0f);
            color.Z = MathC.Clamp(color.Z, 0.0f, 2.0f);
            color.W = MathC.Clamp(color.W, 0.0f, 1.0f);
            return new Vector4(color.X / 2.0f, color.Y / 2.0f, color.Z / 2.0f, color.W);
        }

        public static IReadOnlyList<FileFormat> FileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Wavefront Object", "obj"),
            new FileFormat("Metasequoia", "mqo"),
            new FileFormat("Collada", "dae"),
            new FileFormat("X3D Extensible 3D", "x3d"),
            new FileFormat("glTF 2.0", "gltf")
        };
    }
}
