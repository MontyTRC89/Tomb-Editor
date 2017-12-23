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

        public static IReadOnlyList<FileFormat> FileExtensions { get; } = new List<FileFormat>()
        {
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
