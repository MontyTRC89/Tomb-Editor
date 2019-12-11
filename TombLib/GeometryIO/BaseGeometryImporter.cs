﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public abstract class BaseGeometryImporter
    {
        public delegate Texture GetTextureDelegate(string absoluteFilePath);

        protected IOGeometrySettings _settings { get; set; }
        protected GetTextureDelegate _getTextureCallback { get; }

        public abstract IOModel ImportFromFile(string filename);

        protected void CalculateNormals(IOModel model)
        {
            foreach (var mesh in model.Meshes)
            {
                mesh.Normals.Clear();

                var tempNormals = new List<VertexNormalAverageHelper>();
                for (int i = 0; i < mesh.Positions.Count; i++)
                    tempNormals.Add(new VertexNormalAverageHelper());

                foreach (var submesh in mesh.Submeshes)
                {
                    foreach (var poly in submesh.Value.Polygons)
                    {
                        var p0 = mesh.Positions[poly.Indices[0]];
                        var p1 = mesh.Positions[poly.Indices[1]];
                        var p2 = mesh.Positions[poly.Indices[2]];

                        var v1 = p0 - p2;
                        var v2 = p1 - p2;
                        var normal = Vector3.Cross(v1, v2);

                        tempNormals[poly.Indices[0]].Normal += normal;
                        tempNormals[poly.Indices[0]].NumVertices++;

                        tempNormals[poly.Indices[1]].Normal += normal;
                        tempNormals[poly.Indices[1]].NumVertices++;

                        tempNormals[poly.Indices[2]].Normal += normal;
                        tempNormals[poly.Indices[2]].NumVertices++;

                        if (poly.Shape == IOPolygonShape.Quad)
                        {
                            tempNormals[poly.Indices[3]].Normal += normal;
                            tempNormals[poly.Indices[3]].NumVertices++;
                        }
                    }
                }

                for (int i = 0; i < tempNormals.Count; i++)
                {
                    var normal = tempNormals[i].Normal / Math.Max(1, tempNormals[i].NumVertices);
                    normal = Vector3.Normalize(normal);
                    mesh.Normals.Add(normal);
                }
            }
        }

        public BaseGeometryImporter(IOGeometrySettings settings, GetTextureDelegate getTextureCallback)
        {
            _settings = settings;
            _getTextureCallback = getTextureCallback;
        }

        public static BaseGeometryImporter CreateForFile(string filename, IOGeometrySettings settings, GetTextureDelegate getTextureCallback)
        {
            if (filename.EndsWith(".mqo", StringComparison.InvariantCultureIgnoreCase))
                return new Importers.MetasequoiaImporter(settings, getTextureCallback);
            else
                return new Importers.AssimpImporter(settings, getTextureCallback);
        }

        protected Texture GetTexture(string baseDirectory, string textureFilePath)
        {
            // Try to get texture from absolute file path first, then try to locate texture in same dir as model.
            // If both approaches fail, return null and eventually don't import model.

            string absoluteTextureFilePath = File.Exists(textureFilePath) ? textureFilePath : Path.Combine(baseDirectory, Path.GetFileName(textureFilePath));

            if (!File.Exists(absoluteTextureFilePath))
                return null;

            return (_getTextureCallback != null ? _getTextureCallback(absoluteTextureFilePath) : null);
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
                if (uv.X > 1.0f)
                    uv.X -= (float)Math.Floor(uv.X);
                else if (uv.X < -1.0f)
                    uv.X += -(float)Math.Floor(uv.X);

                if (uv.Y > 1.0f)
                    uv.Y -= (float)Math.Floor(uv.Y);
                else if (uv.Y < -1.0f)
                    uv.Y += -(float)Math.Floor(uv.Y);          
            }
            else
            {
                if (uv.X < -1.0f) uv.X = -1.0f;
                if (uv.Y < -1.0f) uv.Y = -1.0f;
                if (uv.X > 1.0f) uv.X = 1.0f;
                if (uv.Y > 1.0f) uv.Y = 1.0f;
            }

            if (_settings.PremultiplyUV)
            {
                uv.X *= w;
                uv.Y *= h;

                if (!_settings.WrapUV)
                {
                    if (uv.X <= -w) uv.X = -w;
                    if (uv.Y <= -h) uv.Y = -h;
                    if (uv.X >= w) uv.X = w;
                    if (uv.Y >= h) uv.Y = h;
                }
            }
            return uv;
        }

        protected Vector4 ApplyColorTransform(Vector4 color)
        {
            return color;
            //return new Vector4(color.X / 2.0f, color.Y / 2.0f, color.Z / 2.0f, color.W);
        }

        public static IReadOnlyList<FileFormat> AnimationFileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Filmbox", "fbx"),
            new FileFormat("Collada", "dae"),
            new FileFormat("TombEditor XML", "anim")
        };

        public static IReadOnlyList<FileFormat> FileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Metasequoia", "mqo"),
            new FileFormat("Autodesk", "fbx"),
            new FileFormat("Collada", "dae"),
            new FileFormat("glTF", "gltf", "glb"),
            new FileFormat("Blender 3D", "blend"),
            new FileFormat("3ds Max", "3ds", "ase"),
            new FileFormat("Wavefront Object", "obj"),
            new FileFormat("Industry Foundation Classes (IFC / Step),", "ifc"),
            new FileFormat("XGL", "xgl", "zgl"),
            new FileFormat("Stanford Polygon Library", "ply"),
            new FileFormat("AutoCAD DXF", "dxf"),
            new FileFormat("LightWave", "lwo"),
            new FileFormat("LightWave Scene", "lws"),
            new FileFormat("Modo", "lxo"),
            new FileFormat("Stereolithography", "stl"),
            new FileFormat("DirectX X", "x"),
            new FileFormat("AC3D", "ac"),
            new FileFormat("Milkshape 3D", "ms3d"),
            new FileFormat("TrueSpace", "cob", "scn"),
            new FileFormat("OpenGEX", "ogex"),
            new FileFormat("X3D", "x3d"),
            new FileFormat("3MF", "3mf"),
            new FileFormat("Biovision BVH", "bvh"),
            new FileFormat("CharacterStudio Motion", "csm"),
            new FileFormat("Ogre XML", "xml"),
            new FileFormat("Irrlicht Mesh", "irrmesh"),
            new FileFormat("Irrlicht Scene", "irr"),
            new FileFormat("Quake I", "mdl"),
            new FileFormat("Quake II", "md2"),
            new FileFormat("Quake III Mesh", "md3"),
            new FileFormat("Quake III Map / BSP", "pk3"),
            new FileFormat("Return to Castle Wolfenstein", "mdc"),
            new FileFormat("Doom 3", "md5"),
            new FileFormat("Valve Model", "smd", "vta"),
            new FileFormat("Open Game Engine Exchange", "ogex"),
            new FileFormat("Unreal", "3d"),
            new FileFormat("BlitzBasic 3D", "b3d"),
            new FileFormat("Quick3D", "q3d", "q3s"),
            new FileFormat("Neutral File Format", "nff"),
            new FileFormat("Object File Format", "off"),
            new FileFormat("PovRAY Raw", "raw"),
            new FileFormat("Terragen Terrain", "ter"),
            new FileFormat("3D GameStudio(3DGS),", "mdl"),
            new FileFormat("3D GameStudio(3DGS), Terrain", "hmp"),
            new FileFormat("Izware Nendo", "ndo")
        };
    }
}
