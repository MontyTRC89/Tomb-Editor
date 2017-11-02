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
        GeometryImport,
        GeometryExport,
        ObjectForWadTool
    }

    public static class SupportedFormats
    {
        private struct FileFormat
        {
            public string Description;
            public List<string> Extensions;
        }

        private struct FileFormatList
        {
            public FileFormatType ID;
            public List<FileFormat> FileFormats;
        }

        private static List<FileFormatList> _formats = new List<FileFormatList>();

        private static List<FileFormat> GetFormatList(FileFormatType type)
        {
            if(!_formats.Any((id) => id.ID == type))
                _formats.Add(new FileFormatList { ID = type, FileFormats = new List<FileFormat>() });

            return _formats.First((id) => id.ID == type).FileFormats;
        }

        private static void RemoveFormatList(FileFormatType type)
        {
            _formats.RemoveAll((id) => id.ID == type);
        }

        private static bool AddFormat(FileFormatType type, FileFormatType sourceType)
        {
            if (type == sourceType)
                return false;

            List<FileFormat> _destFormatList = GetFormatList(sourceType);
            List<FileFormat> _formatList = GetFormatList(type);

            _formatList.AddRange(_destFormatList);
            return true;
        }

        private static bool AddFormat(FileFormatType type, FileFormatType sourceType, string sourceDescription)
        {
            List<FileFormat> _formatList = GetFormatList(type);
            List<FileFormat> _sourceFormatList = GetFormatList(sourceType);

            if (!_sourceFormatList.Any((desc) => desc.Description.Equals(sourceDescription, StringComparison.InvariantCultureIgnoreCase)))
                return false;
            
            _formatList.Add(_sourceFormatList.First((desc) => desc.Description.Equals(sourceDescription)));
            return true;
        }

        private static bool AddFormat(FileFormatType type, string description, string extension)
        {
            List<FileFormat> _formatList = GetFormatList(type);

            foreach (var format in _formatList)
                if (format.Description.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return false;

            _formatList.Add(new FileFormat() { Description = description, Extensions = new List<string> { extension } });
            return true;
        }

        private static bool AddFormat(FileFormatType type, string description, List<string> extensions)
        {
            List<FileFormat> _formatList = GetFormatList(type);

            foreach(var format in _formatList)
                if (format.Description.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return false;

            _formatList.Add(new FileFormat() { Description = description, Extensions = extensions.ToList() } );
            return true;
        }

        static SupportedFormats()
        {
            // Generate supported imported geometry formats

            AddFormat(FileFormatType.GeometryImport, "Autodesk", "fbx");
            AddFormat(FileFormatType.GeometryImport, "Collada", "dae");
            AddFormat(FileFormatType.GeometryImport, "glTF", new List<string> { "gltf", "glb" });
            AddFormat(FileFormatType.GeometryImport, "Blender 3D", "blend");
            AddFormat(FileFormatType.GeometryImport, "3ds Max", new List<string> { "3ds", "ase" });
            AddFormat(FileFormatType.GeometryImport, "Wavefront Object", "obj");
            AddFormat(FileFormatType.GeometryImport, "Industry Foundation Classes (IFC / Step)", "ifc");
            AddFormat(FileFormatType.GeometryImport, "XGL", new List<string> { "xgl", "zgl" });
            AddFormat(FileFormatType.GeometryImport, "Stanford Polygon Library", "ply");
            AddFormat(FileFormatType.GeometryImport, "AutoCAD DXF", "dxf");
            AddFormat(FileFormatType.GeometryImport, "LightWave", "lwo");
            AddFormat(FileFormatType.GeometryImport, "LightWave Scene", "lws");
            AddFormat(FileFormatType.GeometryImport, "Modo", "lxo");
            AddFormat(FileFormatType.GeometryImport, "Stereolithography", "stl");
            AddFormat(FileFormatType.GeometryImport, "DirectX X", "x");
            AddFormat(FileFormatType.GeometryImport, "AC3D", "ac");
            AddFormat(FileFormatType.GeometryImport, "Milkshape 3D", "ms3d");
            AddFormat(FileFormatType.GeometryImport, "TrueSpace", new List<string> { "cob", "scn" });
            AddFormat(FileFormatType.GeometryImport, "OpenGEX", "ogex");
            AddFormat(FileFormatType.GeometryImport, "X3D", "x3d");
            AddFormat(FileFormatType.GeometryImport, "3MF", "3mf");
            AddFormat(FileFormatType.GeometryImport, "Biovision BVH", "bvh");
            AddFormat(FileFormatType.GeometryImport, "CharacterStudio Motion", "csm");
            AddFormat(FileFormatType.GeometryImport, "Ogre XML", "xml");
            AddFormat(FileFormatType.GeometryImport, "Irrlicht Mesh", "irrmesh");
            AddFormat(FileFormatType.GeometryImport, "Irrlicht Scene", "irr");
            AddFormat(FileFormatType.GeometryImport, "Quake I", "mdl");
            AddFormat(FileFormatType.GeometryImport, "Quake II", "md2");
            AddFormat(FileFormatType.GeometryImport, "Quake III Mesh", "md3");
            AddFormat(FileFormatType.GeometryImport, "Quake III Map / BSP", "pk3");
            AddFormat(FileFormatType.GeometryImport, "Return to Castle Wolfenstein", "mdc");
            AddFormat(FileFormatType.GeometryImport, "Doom 3", "md5");
            AddFormat(FileFormatType.GeometryImport, "Valve Model", new List<string> { "smd", "vta" });
            AddFormat(FileFormatType.GeometryImport, "Open Game Engine Exchange", "ogex");
            AddFormat(FileFormatType.GeometryImport, "Unreal", "3d");
            AddFormat(FileFormatType.GeometryImport, "BlitzBasic 3D", "b3d");
            AddFormat(FileFormatType.GeometryImport, "Quick3D", new List<string> { "q3d", "q3s" });
            AddFormat(FileFormatType.GeometryImport, "Neutral File Format", "nff");
            AddFormat(FileFormatType.GeometryImport, "Object File Format", "off");
            AddFormat(FileFormatType.GeometryImport, "PovRAY Raw", "raw");
            AddFormat(FileFormatType.GeometryImport, "Terragen Terrain", "ter");
            AddFormat(FileFormatType.GeometryImport, "3D GameStudio(3DGS)", "mdl");
            AddFormat(FileFormatType.GeometryImport, "3D GameStudio(3DGS) Terrain", "hmp");
            AddFormat(FileFormatType.GeometryImport, "Izware Nendo", "ndo");

            // Generate supported exported geometry formats

            AddFormat(FileFormatType.GeometryExport, FileFormatType.GeometryImport, "Stanford Polygon Library");
            AddFormat(FileFormatType.GeometryExport, FileFormatType.GeometryImport, "Wavefront Object");
            //AddFormat(FileFormatType.GeometryExport, FileFormatType.GeometryImport, "Autodesk");
            AddFormat(FileFormatType.GeometryExport, "Metasequoia", "mqo");
            AddFormat(FileFormatType.GeometryExport, "Collada", "dae");

            // Generate supported texture formats

            AddFormat(FileFormatType.Texture, "Portable Network Graphics", "png");
            AddFormat(FileFormatType.Texture, "Truevision Targa", "tga");
            AddFormat(FileFormatType.Texture, "Windows Bitmap", "bmp");

            // Generate supported special texture formats

            AddFormat(FileFormatType.SpecialTexture, FileFormatType.Texture);
            AddFormat(FileFormatType.SpecialTexture, "TRLE RAW / PC", new List<string> { "raw", "pc" });

            // Generate supported WAD formats

            AddFormat(FileFormatType.Object, "TRLE WAD", "wad");
            AddFormat(FileFormatType.Object, "TombEditor WAD2", "wad2");

            // Generate supported WadTool source formats

            AddFormat(FileFormatType.ObjectForWadTool, FileFormatType.Object);
            AddFormat(FileFormatType.ObjectForWadTool, "Tomb Raider I level", "phd");
            AddFormat(FileFormatType.ObjectForWadTool, "Tomb Raider II/III level", "tr2");
            AddFormat(FileFormatType.ObjectForWadTool, "Tomb Raider The Last Revelation level", "tr4");
            AddFormat(FileFormatType.ObjectForWadTool, "Tomb Raider Chronicles level", "trc");
        }

        public static bool IsExtensionPresent(FileFormatType type, string extension)
        {
            List<FileFormat> _formatList = GetFormatList(type);
            
            if (_formatList.Count == 0)
                return false;

            foreach (var format in _formatList)
                if (format.Extensions.Exists(item => extension.EndsWith("." + item, StringComparison.InvariantCultureIgnoreCase)))
                    return true;

            return false;
        }

        public static string GetFilter(FileFormatType type, bool allTypes = true, bool anyFile = true)
        {
            const string allFiles = "All files (*.*)|*.*";

            List<FileFormat> _formatList = GetFormatList(type);

            if (_formatList.Count == 0)
                return allFiles;

            int numExtensions = _formatList.Count;
            string result = "";

            if(allTypes)
            {
                result += "Any supported format|";

                for(int i = 0; i < _formatList.Count; i++)
                {
                    for(int j = 0; j < _formatList[i].Extensions.Count; j++)
                    {
                        result += "*." + _formatList[i].Extensions[j];

                        if (j != _formatList[i].Extensions.Count - 1)
                            result += ";";
                    }

                    if (i != _formatList.Count - 1)
                        result += ";";
                }

                result += "|";
            }

            for (int i = 0; i < _formatList.Count; i++)
            {
                string finalDescription = _formatList[i].Description + " (";

                for (int j = 0; j < _formatList[i].Extensions.Count; j++)
                {
                    finalDescription = finalDescription + "*." + _formatList[i].Extensions[j];

                    if (j == _formatList[i].Extensions.Count - 1)
                        finalDescription = finalDescription + ")";
                    else
                        finalDescription = finalDescription + ", ";
                }

                result += finalDescription + "|";

                for (int j = 0; j < _formatList[i].Extensions.Count; j++)
                {
                    result += "*." + _formatList[i].Extensions[j];

                    if (j != _formatList[i].Extensions.Count - 1)
                        result += ";";
                }

                if (i != _formatList.Count - 1)
                    result += "|";
            }

            if(anyFile)
                result += "|" + allFiles;

            return result;
        }

        public static string GetExtensionFromIndex(FileFormatType type, int index = 0)
        {
            List<FileFormat> _formatList = GetFormatList(type);

            if (_formatList.Count == 0 || _formatList.Count <= index)
                return "";

            return _formatList[index].Extensions.First();
        }
    }
}