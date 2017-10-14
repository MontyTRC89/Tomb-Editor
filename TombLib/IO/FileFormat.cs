using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.IO
{
    public enum FileFormatType
    {
        Object,
        Texture,
        SpecialTexture,
        Geometry
    }

    public static class SupportedFormats
    {
        private struct FileFormat
        {
            public string Description;
            public List<string> Extensions;
        }

        private static List<FileFormat> _supportedGeometryFormats = new List<FileFormat>();
        private static List<FileFormat> _supportedTextureFormats = new List<FileFormat>();
        private static List<FileFormat> _supportedSpecialTextureFormats = new List<FileFormat>();
        private static List<FileFormat> _supportedWadFormats = new List<FileFormat>();

        private static List<FileFormat> GetFormatList(FileFormatType type)
        {
            switch (type)
            {
                case FileFormatType.Object:
                    return _supportedWadFormats;
                case FileFormatType.Texture:
                    return _supportedTextureFormats;
                case FileFormatType.SpecialTexture:
                    return _supportedSpecialTextureFormats;
                case FileFormatType.Geometry:
                    return _supportedGeometryFormats;
                default:
                    return null;
            }
        }

        private static bool AddFormat(FileFormatType type, string description, string extension)
        {
            List<FileFormat> _formatList = GetFormatList(type);

            foreach (var format in _formatList)
                if (format.Description.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return false;

            _formatList.Add(new FileFormat() { Description = description + " (*." + extension + ")", Extensions = new List<string> { extension } });
            return true;
        }

        private static bool AddFormat(FileFormatType type, string description, List<string> extensions)
        {
            List<FileFormat> _formatList = GetFormatList(type);

            string _finalDescription = description + " (";

            foreach(var format in _formatList)
                if (format.Description.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return false;

            foreach (var ext in extensions)
            {
                _finalDescription = _finalDescription + "*." + ext;

                if (extensions.Last().Equals(ext, StringComparison.InvariantCultureIgnoreCase))
                    _finalDescription = _finalDescription + ")";
                else
                    _finalDescription = _finalDescription + ", ";
            }

            _formatList.Add(new FileFormat() { Description = _finalDescription, Extensions = extensions.ToList() } );
            return true;
        }

        static SupportedFormats()
        {
            // Generate supported geometry formats

            AddFormat(FileFormatType.Geometry, "Autodesk", "fbx");
            AddFormat(FileFormatType.Geometry, "Collada", "dae");
            AddFormat(FileFormatType.Geometry, "glTF", new List<string> { "gltf", "glb" });
            AddFormat(FileFormatType.Geometry, "Blender 3D", "blend");
            AddFormat(FileFormatType.Geometry, "3ds Max", new List<string> { "3ds", "ase" });
            AddFormat(FileFormatType.Geometry, "Wavefront Object", "obj");
            AddFormat(FileFormatType.Geometry, "Industry Foundation Classes (IFC / Step)", "ifc");
            AddFormat(FileFormatType.Geometry, "XGL", new List<string> { "xgl", "zgl" });
            AddFormat(FileFormatType.Geometry, "Stanford Polygon Library", "ply");
            AddFormat(FileFormatType.Geometry, "AutoCAD DXF", "dxf");
            AddFormat(FileFormatType.Geometry, "LightWave", "lwo");
            AddFormat(FileFormatType.Geometry, "LightWave Scene", "lws");
            AddFormat(FileFormatType.Geometry, "Modo", "lxo");
            AddFormat(FileFormatType.Geometry, "Stereolithography", "stl");
            AddFormat(FileFormatType.Geometry, "DirectX X", "x");
            AddFormat(FileFormatType.Geometry, "AC3D", "ac");
            AddFormat(FileFormatType.Geometry, "Milkshape 3D", "ms3d");
            AddFormat(FileFormatType.Geometry, "TrueSpace", new List<string> { "cob", "scn" });
            AddFormat(FileFormatType.Geometry, "OpenGEX", "ogex");
            AddFormat(FileFormatType.Geometry, "X3D", "x3d");
            AddFormat(FileFormatType.Geometry, "3MF", "3mf");
            AddFormat(FileFormatType.Geometry, "Biovision BVH", "bvh");
            AddFormat(FileFormatType.Geometry, "CharacterStudio Motion", "csm");
            AddFormat(FileFormatType.Geometry, "Ogre XML", "xml");
            AddFormat(FileFormatType.Geometry, "Irrlicht Mesh", "irrmesh");
            AddFormat(FileFormatType.Geometry, "Irrlicht Scene", "irr");
            AddFormat(FileFormatType.Geometry, "Quake I", "mdl");
            AddFormat(FileFormatType.Geometry, "Quake II", "md2");
            AddFormat(FileFormatType.Geometry, "Quake III Mesh", "md3");
            AddFormat(FileFormatType.Geometry, "Quake III Map / BSP", "pk3");
            AddFormat(FileFormatType.Geometry, "Return to Castle Wolfenstein", "mdc");
            AddFormat(FileFormatType.Geometry, "Doom 3", "md5");
            AddFormat(FileFormatType.Geometry, "Valve Model", new List<string> { "smd", "vta" });
            AddFormat(FileFormatType.Geometry, "Open Game Engine Exchange", "ogex");
            AddFormat(FileFormatType.Geometry, "Unreal", "3d");
            AddFormat(FileFormatType.Geometry, "BlitzBasic 3D", "b3d");
            AddFormat(FileFormatType.Geometry, "Quick3D", new List<string> { "q3d", "q3s" });
            AddFormat(FileFormatType.Geometry, "Neutral File Format", "nff");
            AddFormat(FileFormatType.Geometry, "Object File Format", "off");
            AddFormat(FileFormatType.Geometry, "PovRAY Raw", "raw");
            AddFormat(FileFormatType.Geometry, "Terragen Terrain", "ter");
            AddFormat(FileFormatType.Geometry, "3D GameStudio(3DGS)", "mdl");
            AddFormat(FileFormatType.Geometry, "3D GameStudio(3DGS) Terrain", "hmp");
            AddFormat(FileFormatType.Geometry, "Izware Nendo", "ndo");

            // Generate supported texture formats

            AddFormat(FileFormatType.Texture, "Portable Network Graphics", "png");
            AddFormat(FileFormatType.Texture, "Truevision Targa", "tga");
            AddFormat(FileFormatType.Texture, "Windows Bitmap", "bmp");

            // Generate supported special texture formats

            AddFormat(FileFormatType.SpecialTexture, "TRLE RAW / PC", new List<string> { "raw", "pc" });
            AddFormat(FileFormatType.SpecialTexture, "Portable Network Graphics", "png");
            AddFormat(FileFormatType.SpecialTexture, "Truevision Targa", "tga");
            AddFormat(FileFormatType.SpecialTexture, "Windows Bitmap", "bmp");

            // Generate supported WAD formats

            AddFormat(FileFormatType.Object, "TRLE WAD", "wad");
            AddFormat(FileFormatType.Object, "TombEditor WAD2", "wad2");
        }

        public static bool IsExtensionPresent(FileFormatType type, string extension)
        {
            List<FileFormat> _formatList = GetFormatList(type);

            foreach (var format in _formatList)
                if (format.Extensions.Exists(item => extension.EndsWith("." + item, StringComparison.InvariantCultureIgnoreCase)))
                    return true;

            return false;
        }

        public static string GetFilter(FileFormatType type)
        {
            List<FileFormat> _formatList = GetFormatList(type);

            int numExtensions = _formatList.Count;
            string result = "Any supported format|";

            foreach (var format in _formatList)
            {
                foreach (var ext in format.Extensions)
                    result = result + "*." + ext;

                if (format.Equals(_formatList.Last()))
                    result = result + "|";
                else
                    result = result + ";";
            }

            foreach (var format in _formatList)
            {
                result = result + format.Description + "|";

                foreach (var ext in format.Extensions)
                {
                    result = result + ext;

                    if (ext.Equals(ext.Last()))
                        result = result + "|";
                    else
                        result = result + ";";
                }

                if (!format.Equals(_formatList.Last()))
                    result = result + "|";
            }

            result = result + "|All files (*.*)|*";

            return result;
        }
    }
}