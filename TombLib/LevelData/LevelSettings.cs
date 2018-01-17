using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using ImportedGeometryUpdateInfo = System.Collections.Generic.KeyValuePair<TombLib.LevelData.ImportedGeometry, TombLib.LevelData.ImportedGeometryInfo>;

namespace TombLib.LevelData
{
    public enum VariableType
    {
        [Description("The directory of the *.prj2 file.")]
        LevelDirectory,
        [Description("The directory of the .txt files for script")]
        ScriptDirectory,
        [Description("The directory in which all game components reside.")]
        GameDirectory,
        [Description("The directory of the editor application.")]
        EditorDirectory,
        [Description("The name of the *.prj2 file.")]
        LevelName,
        None,
    }

    public class OldWadSoundPath : ICloneable
    {
        public string Path { get; set; }

        public OldWadSoundPath(string path)
        {
            Path = path;
        }

        public OldWadSoundPath Clone()
        {
            return (OldWadSoundPath)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    /// <summary>
    /// Note: This enumeration can *always* legally be in a state not yet listed here. We should always handle *default* in switch.
    /// </summary>
    public enum GameVersion : long
    {
        //TR1 = 1,
        //TR2 = 2,
        //TR3 = 3,
        TR4 = 4,
        TR5 = 5,
        TRNG = 16
    }

    /// <summary>
    /// Only for TR5
    /// </summary>
    public enum Tr5LaraType : byte
    {
        Normal = 0,
        Catsuit = 3,
        Divesuit = 4,
        Invisible = 6
    }

    /// <summary>
    /// Only for TR5
    /// </summary>
    public enum Tr5WeatherType : byte
    {
        Normal = 0,
        Rain = 1,
        Snow = 2
    }

    public class LevelSettings : ICloneable
    {
        public const string VariableBegin = "$(";
        public const string VariableEnd = ")";
        public static readonly char Dir = Path.DirectorySeparatorChar;

        public string LevelFilePath { get; set; } = null; // Can be null if the level has not been loaded from / saved to disk yet.
        public string WadFilePath { get; set; } = null; // Can be null if no object file is loaded.
        public string FontTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string SkyTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string Tr5ExtraSpritesFilePath { get; set; } = null; // Can be null if the default should be used.
        public List<OldWadSoundPath> OldWadSoundPaths { get; set; } = new List<OldWadSoundPath>
            {
                new OldWadSoundPath("Sounds"),
                new OldWadSoundPath(""),
                new OldWadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + "sound\\Samples"),
                new OldWadSoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds\\Samples")
            };

        public string ScriptDirectory { get; set; } = VariableCreate(VariableType.EditorDirectory) + Dir + "Script";
        public string GameDirectory { get; set; } = VariableCreate(VariableType.EditorDirectory) + Dir + "Game";
        public string GameLevelFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "data" + Dir + VariableCreate(VariableType.LevelName) + ".tr4"; // Relative to "GameDirectory"
        public string GameExecutableFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "Tomb4.exe"; // Relative to "GameDirectory"
        public bool GameEnableQuickStartFeature { get; set; } = true;
        public GameVersion GameVersion { get; set; } = GameVersion.TR4;
        public List<LevelTexture> Textures { get; set; } = new List<LevelTexture>();
        public List<AnimatedTextureSet> AnimatedTextureSets { get; set; } = new List<AnimatedTextureSet>();
        public List<ImportedGeometry> ImportedGeometries { get; set; } = new List<ImportedGeometry>();

        public Vector4 DefaultAmbientLight { get; set; } = new Vector4(0.25f, 0.25f, 0.25f, 2.0f);

        // For TR5 only
        public Tr5LaraType Tr5LaraType { get; set; }
        public Tr5WeatherType Tr5WeatherType { get; set; }

        public LevelSettings()
        {
            
        }

        public LevelSettings Clone()
        {
            LevelSettings result = (LevelSettings)MemberwiseClone();
            result.OldWadSoundPaths = OldWadSoundPaths.ConvertAll((soundPath) => soundPath.Clone());
            result.Textures = Textures.ConvertAll((texture) => (LevelTexture)(texture.Clone()));
            result.AnimatedTextureSets = AnimatedTextureSets.ConvertAll((set) => set.Clone());
            result.ImportedGeometries = ImportedGeometries.ConvertAll((geometry) => geometry.Clone());
            return result;
        }

        object ICloneable.Clone() => Clone();

        public static string VariableCreate(VariableType type)
        {
            return VariableBegin + type.ToString() + VariableEnd;
        }

        public string GetVariable(VariableType type)
        {
            switch (type)
            {
                case VariableType.None:
                    return "";
                case VariableType.EditorDirectory:
                    return Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
                case VariableType.ScriptDirectory:
                    return Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location) + Dir + "Script";
                case VariableType.LevelDirectory:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                        return Path.GetDirectoryName(LevelFilePath);
                    return GetVariable(VariableType.EditorDirectory);
                case VariableType.GameDirectory:
                    return MakeAbsolute(GameDirectory ?? VariableCreate(VariableType.LevelDirectory), VariableType.GameDirectory);
                case VariableType.LevelName:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                        return FileSystemUtils.GetFileNameWithoutExtensionTry(LevelFilePath);
                    if (!string.IsNullOrEmpty(WadFilePath))
                        return FileSystemUtils.GetFileNameWithoutExtensionTry(WadFilePath);
                    return "Default";
                default:
                    throw new ArgumentException();
            }
        }

        public string ParseVariables(string path, params VariableType[] excluded)
        {
            int startIndex = 0;
            do
            {
                // Find variable
                startIndex = path.IndexOf(VariableBegin, startIndex);
                if (startIndex == -1)
                    break;
                int afterStartIndex = startIndex + VariableBegin.Length;
                int endIndex = path.IndexOf(VariableEnd, afterStartIndex);
                if (endIndex == -1)
                    break;
                string variableName = path.Substring(afterStartIndex, endIndex - afterStartIndex);

                // Parse variable
                VariableType variableType;
                if ((!Enum.TryParse(variableName, out variableType)) ||
                    excluded.Contains(variableType))
                {
                    startIndex = endIndex + VariableEnd.Length;
                    continue;
                }
                string variableContent = GetVariable(variableType);
                path = path.Remove(startIndex, (endIndex + VariableEnd.Length) - startIndex);
                path = path.Insert(startIndex, variableContent);
                startIndex += variableContent.Length;
            } while (true);

            return path;
        }

        public string TextureFilePath
        {
            get
            {
                return Textures.Count > 0 ? Textures[0].Path : "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Textures.Clear();
                else if (Textures.Count > 0)
                    Textures[0].SetPath(this, value);
                else
                    Textures.Add(new LevelTexture(this, value));
            }
        }

        public string MakeAbsolute(string path, params VariableType[] excluded)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = ParseVariables(path, excluded);

            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return path;
            }
        }

        public string MakeRelative(string path, VariableType baseDirType)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                if (string.IsNullOrEmpty(LevelFilePath))
                    return Path.GetFullPath(path);

                switch (baseDirType)
                {
                    case VariableType.None:
                        return Path.GetFullPath(path);
                    case VariableType.EditorDirectory:
                    case VariableType.GameDirectory:
                    case VariableType.LevelDirectory:
                    case VariableType.ScriptDirectory:
                        string relativePath = PathC.GetRelativePath(GetVariable(baseDirType), path);
                        if (relativePath == null)
                            return Path.GetFullPath(path);
                        return VariableCreate(baseDirType) + Path.DirectorySeparatorChar + relativePath;
                    default:
                        return path;
                }
            }
            catch
            {
                return path;
            }
        }

        public string FontTextureFileNameAbsoluteOrDefault => MakeAbsolute(FontTextureFilePath) ??
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location), "Editor/Textures/Font.pc.png");

        public string SkyTextureFileNameAbsoluteOrDefault => MakeAbsolute(SkyTextureFilePath) ??
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location), "Editor/Textures/pcsky.raw.png");

        public string Tr5ExtraSpritesFileNameAbsoluteOrDefault => MakeAbsolute(Tr5ExtraSpritesFilePath) ??
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location), "Editor/Textures/Extra.Tr5.pc.png");

        public static ImageC LoadRawExtraTexture(string path)
        {
            using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ImageC image;

                if (path.EndsWith(".raw"))
                {
                    // Raw file: 256² pixels with 24 bpp
                    byte[] data = new byte[256 * 256 * 3];
                    reader.Read(data, 0, data.Length);

                    image = ImageC.CreateNew(256, 256);
                    for (int i = 0; i < 256 * 256; ++i)
                        image.Set(i, data[i * 3], data[i * 3 + 1], data[i * 3 + 2]);
                }
                else if (path.EndsWith(".pc"))
                {
                    // Raw file: 256² pixels with 32 bpp
                    byte[] data = new byte[256 * 256 * 4];
                    reader.Read(data, 0, data.Length);

                    image = ImageC.CreateNew(256, 256);
                    for (int i = 0; i < 256 * 256; ++i)
                        image.Set(i, data[i * 4 + 2], data[i * 4 + 1], data[i * 4], data[i * 4 + 3]);
                }
                else
                    image = ImageC.FromStream(reader);

                if ((image.Width != 256) || (image.Height != 256))
                    throw new NotSupportedException("The texture's size must be 256 by 256 pixels. " +
                        "(The current texture '" + path + "' is " + image.Width + " by " + image.Height + " pixels)");
                return image;
            }
        }

        public static IEnumerable<FileFormat> LoadRawExtraTextureFileFormats =>
            new FileFormat[1] { new FileFormat("Raw sky/font image", "raw", "pc") }.Concat(ImageC.FromFileFileExtensions);


        public string LookupSound(string soundName, bool ignoreMissingSounds)
        {
            foreach (var soundPath in OldWadSoundPaths)
            {
                string realPath = Path.Combine(MakeAbsolute(soundPath.Path), soundName);
                if (File.Exists(realPath))
                    return realPath;
            }
            if (ignoreMissingSounds)
                return null;
            throw new FileNotFoundException("Sound not found", soundName);
        }

        public byte[] ReadSound(string sound, bool ignoreMissingSounds)
        {
            string path = LookupSound(sound, ignoreMissingSounds);
            if (string.IsNullOrEmpty(path))
                return new byte[0];

            // Try opening sound
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] result = new byte[checked((int)fileStream.Length)];
                fileStream.Read(result, 0, result.GetLength(0));
                return result;
            }
        }

        public void ImportedGeometryUpdate(IEnumerable<ImportedGeometryUpdateInfo> geometriesToUpdate)
        {
            var absolutePathTextureLookup = new Dictionary<string, TombLib.Utils.Texture>();

            // Add level textures to lookup
            foreach (LevelTexture levelTexture in Textures)
            {
                if (string.IsNullOrEmpty(levelTexture.Path))
                    continue;
                string absolutePath = MakeAbsolute(levelTexture.Path);
                if (!absolutePathTextureLookup.ContainsKey(absolutePath))
                    absolutePathTextureLookup.Add(absolutePath, levelTexture);
            }

            // Add other imported geometry texture to lookup
            foreach (ImportedGeometry importedGeometry in ImportedGeometries)
                foreach (ImportedGeometryTexture importedGeometryTexture in importedGeometry.Textures)
                    if (!absolutePathTextureLookup.ContainsKey(importedGeometryTexture.AbsolutePath))
                        absolutePathTextureLookup.Add(importedGeometryTexture.AbsolutePath, importedGeometryTexture);

            // Load geometries
            foreach (ImportedGeometryUpdateInfo geometryToUpdate in geometriesToUpdate)
                geometryToUpdate.Key.Update(this, absolutePathTextureLookup, geometryToUpdate.Value);
        }

        public void ImportedGeometryUpdate(ImportedGeometry geometry, ImportedGeometryInfo info)
        {
            ImportedGeometryUpdate(new ImportedGeometryUpdateInfo[] { new ImportedGeometryUpdateInfo(geometry, info) });
        }

        public ImportedGeometry ImportedGeometryFromID(ImportedGeometry.UniqueIDType uniqueID)
        {
            foreach (ImportedGeometry importedGeometry in ImportedGeometries)
                if (importedGeometry.UniqueID == uniqueID)
                    return importedGeometry;
            return null;
        }
    }
}
