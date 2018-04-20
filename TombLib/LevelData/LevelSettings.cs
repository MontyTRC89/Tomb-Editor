using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;
using ImportedGeometryUpdateInfo = System.Collections.Generic.KeyValuePair<TombLib.LevelData.ImportedGeometry, TombLib.LevelData.ImportedGeometryInfo>;

namespace TombLib.LevelData
{
    public enum VariableType
    {
        [Description("The directory of the main level file (*.prj2).")]
        LevelDirectory,
        [Description("The directory of the .txt files for script")]
        ScriptDirectory,
        [Description("The directory in which all game components reside.")]
        GameDirectory,
        [Description("The directory of the editor application.")]
        EditorDirectory,
        [Description("The name of the main level file (*.prj2).")]
        LevelName,
        [Description("The engine.")]
        EngineVersion,
        [Description("The sound engine (Outputs TR4 for the 'TRNG' engine).")]
        SoundEngineVersion
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
        TR2 = 2,
        TR3 = 3,
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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const string VariableBegin = "$(";
        public const string VariableEnd = ")";
        public static readonly char Dir = Path.DirectorySeparatorChar;

        public string LevelFilePath { get; set; } = null; // Can be null if the level has not been loaded from / saved to disk yet.
        public string FontTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string SkyTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string Tr5ExtraSpritesFilePath { get; set; } = null; // Can be null if the default should be used.

        public List<ReferencedWad> Wads { get; set; } = new List<ReferencedWad>();

        public List<OldWadSoundPath> OldWadSoundPaths { get; set; } = new List<OldWadSoundPath>
            {
                new OldWadSoundPath(""),
                new OldWadSoundPath("Sounds"), // For directly loading wad files.
                new OldWadSoundPath("Sound"),
                new OldWadSoundPath("Sounds/Samples"),
                new OldWadSoundPath("Sound/Samples"),
                new OldWadSoundPath("../Sounds"), // For directly loading wad files.
                new OldWadSoundPath("../Sound"),
                new OldWadSoundPath("../Sounds/Samples"),
                new OldWadSoundPath("../Sound/Samples"),
                new OldWadSoundPath("../../Sounds"), // For directly loading wad files.
                new OldWadSoundPath("../../Sound"),
                new OldWadSoundPath("../../Sounds/Samples"),
                new OldWadSoundPath("../../Sound/Samples"),
                new OldWadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + "sound" + Dir + "Samples"),
                new OldWadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + ".." + Dir + "sound" + Dir + "Samples"),
                new OldWadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + ".." + Dir + ".." + Dir + "sound" + Dir + "Samples"),
                new OldWadSoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds" + Dir + VariableCreate(VariableType.SoundEngineVersion) + Dir + "Samples"),
                new OldWadSoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds" + Dir + "Samples")
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
        public Vector4 DefaultAmbientLight { get; set; } = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);

        // For TR5 only
        public Tr5LaraType Tr5LaraType { get; set; } = Tr5LaraType.Normal;
        public Tr5WeatherType Tr5WeatherType { get; set; } = Tr5WeatherType.Normal;

        public LevelSettings Clone()
        {
            LevelSettings result = (LevelSettings)MemberwiseClone();
            result.Wads = Wads.ConvertAll(wad => wad.Clone());
            result.OldWadSoundPaths = OldWadSoundPaths.ConvertAll(soundPath => soundPath.Clone());
            result.Textures = Textures.ConvertAll(texture => (LevelTexture)texture.Clone());
            result.AnimatedTextureSets = AnimatedTextureSets.ConvertAll(set => set.Clone());
            result.ImportedGeometries = ImportedGeometries.ConvertAll(geometry => geometry.Clone());
            return result;
        }

        object ICloneable.Clone() => Clone();

        public static string VariableCreate(VariableType type)
        {
            return VariableBegin + type + VariableEnd;
        }

        public string GetVariable(VariableType type)
        {
            switch (type)
            {
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
                    if (Wads.Count > 0 && !string.IsNullOrEmpty(Wads[0].Path))
                        return FileSystemUtils.GetFileNameWithoutExtensionTry(Wads[0].Path);
                    return "Default";
                case VariableType.EngineVersion:
                    return GameVersion.ToString();
                case VariableType.SoundEngineVersion:
                    return (GameVersion == GameVersion.TRNG ? GameVersion.TR4 : GameVersion).ToString();
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
                if (!Enum.TryParse(variableName, out variableType) ||
                    excluded.Contains(variableType))
                {
                    startIndex = endIndex + VariableEnd.Length;
                    continue;
                }
                string variableContent = GetVariable(variableType);
                path = path.Remove(startIndex, endIndex + VariableEnd.Length - startIndex);
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

        public WadGameVersion WadGameVersion
        {
            get
            {
                switch (GameVersion)
                {
                    case GameVersion.TR2:
                        return Wad.WadGameVersion.TR2;
                    case GameVersion.TR3:
                        return Wad.WadGameVersion.TR3;
                    case GameVersion.TR4:
                    case GameVersion.TRNG:
                        return Wad.WadGameVersion.TR4_TRNG;
                    case GameVersion.TR5:
                        return Wad.WadGameVersion.TR5;
                    default:
                        throw new NotSupportedException("Not supported game version.");
                }
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

        public Wad2 ComposeActualWad()
        {
            Wad2 result = new Wad2();
            int TODO_ComposeActualWad;
            throw new NotImplementedException();
        }

        public ImageC LoadFontTexture()
        {
            string absolutePath = MakeAbsolute(FontTextureFilePath);
            if (string.IsNullOrEmpty(absolutePath))
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.Font_pc);
            return LoadRawExtraTexture(absolutePath);
        }

        public ImageC LoadSkyTexture()
        {
            string absolutePath = MakeAbsolute(SkyTextureFilePath);
            if (string.IsNullOrEmpty(absolutePath))
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.pcsky_raw);
            return LoadRawExtraTexture(absolutePath);
        }

        public ImageC LoadTr5ExtraSprites()
        {
            string absolutePath = MakeAbsolute(Tr5ExtraSpritesFilePath);
            if (string.IsNullOrEmpty(absolutePath))
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.Extra_Tr5_pc);
            return LoadRawExtraTexture(absolutePath);
        }

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

                if (image.Width != 256 || image.Height != 256)
                    throw new NotSupportedException("The texture's size must be 256 by 256 pixels. " +
                        "(The current texture '" + path + "' is " + image.Width + " by " + image.Height + " pixels)");
                return image;
            }
        }

        public void ImportedGeometryUpdate(IEnumerable<ImportedGeometryUpdateInfo> geometriesToUpdate)
        {
            var absolutePathTextureLookup = new Dictionary<string, Texture>();

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
            ImportedGeometryUpdate(new[] { new ImportedGeometryUpdateInfo(geometry, info) });
        }

        public ImportedGeometry ImportedGeometryFromID(ImportedGeometry.UniqueIDType uniqueID)
        {
            foreach (ImportedGeometry importedGeometry in ImportedGeometries)
                if (importedGeometry.UniqueID == uniqueID)
                    return importedGeometry;
            return null;
        }

        public WadMoveable WadTryGetMoveable(WadMoveableId id)
        {
            WadMoveable result;
            foreach (ReferencedWad wad in Wads)
                if (wad.Wad != null && wad.Wad.Moveables.TryGetValue(id, out result))
                    return result;
            return null;
        }

        public WadStatic WadTryGetStatic(WadStaticId id)
        {
            WadStatic result;
            foreach (ReferencedWad wad in Wads)
                if (wad.Wad != null && wad.Wad.Statics.TryGetValue(id, out result))
                    return result;
            return null;
        }

        public WadFixedSoundInfo WadTryGetFixedSoundInfo(WadFixedSoundInfoId id)
        {
            WadFixedSoundInfo result;
            foreach (ReferencedWad wad in Wads)
                if (wad.Wad != null && wad.Wad.FixedSoundInfos.TryGetValue(id, out result))
                    return result;
            return null;
        }

        public WadSoundInfo WadTryGetSoundInfo(string soundName)
        {
            foreach (ReferencedWad wad in Wads)
                if (wad.Wad != null)
                {
                    WadSoundInfo result = wad.Wad.TryGetSound(soundName);
                    if (result != null)
                        return result;
                }
            return null;
        }

        public SortedList<WadMoveableId, WadMoveable> WadGetAllMoveables()
        {
            SortedList<WadMoveableId, WadMoveable> result = new SortedList<WadMoveableId, WadMoveable>();
            foreach (ReferencedWad wad in Wads)
                foreach (KeyValuePair<WadMoveableId, WadMoveable> moveable in wad.Wad.Moveables)
                    if (!result.ContainsKey(moveable.Key))
                        result.Add(moveable.Key, moveable.Value);
            return result;
        }

        public SortedList<WadStaticId, WadStatic> WadGetAllStatics()
        {
            SortedList<WadStaticId, WadStatic> result = new SortedList<WadStaticId, WadStatic>();
            foreach (ReferencedWad wad in Wads)
                foreach (KeyValuePair<WadStaticId, WadStatic> @static in wad.Wad.Statics)
                    if (!result.ContainsKey(@static.Key))
                        result.Add(@static.Key, @static.Value);
            return result;
        }

        public SortedList<WadSpriteSequenceId, WadSpriteSequence> WadGetAllSpriteSequences()
        {
            SortedList<WadSpriteSequenceId, WadSpriteSequence> result = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
            foreach (ReferencedWad wad in Wads)
                foreach (KeyValuePair<WadSpriteSequenceId, WadSpriteSequence> moveable in wad.Wad.SpriteSequences)
                    if (!result.ContainsKey(moveable.Key))
                        result.Add(moveable.Key, moveable.Value);
            return result;
        }

        public SortedList<WadFixedSoundInfoId, WadFixedSoundInfo> WadGetAllFixedSoundInfos()
        {
            SortedList<WadFixedSoundInfoId, WadFixedSoundInfo> result = new SortedList<WadFixedSoundInfoId, WadFixedSoundInfo>();
            foreach (ReferencedWad wad in Wads)
                foreach (KeyValuePair<WadFixedSoundInfoId, WadFixedSoundInfo> moveable in wad.Wad.FixedSoundInfos)
                    if (!result.ContainsKey(moveable.Key))
                        result.Add(moveable.Key, moveable.Value);
            return result;
        }

        public HashSet<WadSoundInfo> WadGetAllSoundInfos()
        {
            HashSet<WadSoundInfo> result = new HashSet<WadSoundInfo>();
            foreach (ReferencedWad wad in Wads)
                result.UnionWith(wad.Wad.SoundInfosUnique);
            return result;
        }

        public static IEnumerable<FileFormat> FileFormatsLoadRawExtraTexture =>
            new[] { new FileFormat("Raw sky/font image", "raw", "pc") }.Concat(ImageC.FromFileFileExtensions);
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevel = new[] { new FileFormat("Tomb Editor Level", "prj2") };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevelPrj = new[] { new FileFormat("Tomb Editor Level", "prj") };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevelCompiled = new[]
        {
            new FileFormat("Tomb Raider I level", "phd"),
            new FileFormat("Tomb Raider II/III level", "tr2"),
            new FileFormat("Tomb Raider The Last Revelation level", "tr4"),
            new FileFormat("Tomb Raider Chronicles level", "trc")
        };
    }
}
