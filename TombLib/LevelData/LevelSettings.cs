using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

    public class AutoStaticMeshMergeEntry : ICloneable, IEquatable<AutoStaticMeshMergeEntry>
    {
        public string StaticMesh
        {
            get { return parent.WadTryGetStatic(new WadStaticId(meshId)).ToString(parent.GameVersion); }
        }

        private LevelSettings parent;
        public uint meshId;
        public bool Merge { get; set; }
        public bool InterpretShadesAsEffect { get; set; }
        public bool TintAsAmbient { get; set; }
        public bool ClearShades { get; set; }
        public AutoStaticMeshMergeEntry(uint staticMesh, bool merge, bool interpretShadesAsEffect, bool tintAsAmbient, bool clearShades, LevelSettings parent)
        {
            this.meshId = staticMesh;
            this.parent = parent;
            this.Merge = merge;
            this.TintAsAmbient = tintAsAmbient;
            this.InterpretShadesAsEffect = interpretShadesAsEffect;
            this.ClearShades = clearShades;
        }

        public AutoStaticMeshMergeEntry Clone()
        {
            return (AutoStaticMeshMergeEntry)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override int GetHashCode()
        {
            return ("merged" + meshId + ClearShades + InterpretShadesAsEffect + Merge + TintAsAmbient).GetHashCode();
        }

        public override bool Equals(Object other) => Equals(other as AutoStaticMeshMergeEntry);

        public bool Equals(AutoStaticMeshMergeEntry other)
        {
            if (other == null)
                return false;

            return (other.meshId == meshId &&
                    other.ClearShades == ClearShades &&
                    other.InterpretShadesAsEffect == InterpretShadesAsEffect &&
                    other.Merge == Merge &&
                    other.TintAsAmbient == TintAsAmbient);
        }
    }

    public class WadSoundPath : ICloneable
    {
        public string Path { get; set; }

        public WadSoundPath(string path)
        {
            Path = path;
        }

        public WadSoundPath Clone()
        {
            return (WadSoundPath)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public class LevelSettings : ICloneable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const string VariableBegin = "$(";
        public const string VariableEnd = ")";
        public static readonly char Dir = Path.DirectorySeparatorChar;

        // Identify if current level was loaded from prj2 with unknown chunks, possibly
        // indicating that data loss occured.
        public bool HasUnknownData { get; set; } = false;

        // Last used room
        public int LastSelectedRoom { get; set; } = 0;

        // New sound system
        public SoundSystem SoundSystem { get; set; } = SoundSystem.Xml;
        public List<int> SelectedSounds { get; set; } = new List<int>();
        public List<WadSoundInfo> GlobalSoundMap
        {
            get
            {
                var soundmap = new SortedDictionary<int, WadSoundInfo>();

                // Loop through all reference classes, collecting sounds
                foreach (var soundsRef in SoundsCatalogs)
                    if (soundsRef.LoadException == null)
                        foreach (var sound in soundsRef.Sounds.SoundInfos)
                            if (!soundmap.ContainsKey(sound.Id))
                                soundmap.Add(sound.Id, sound);

                return soundmap.Values.ToList();
            }
        }
        public bool AutoAssignSoundsIfNoSelection { get; set; } = true; // Autodetect and assign sounds if none selected

        // Default sound paths
        public List<WadSoundPath> WadSoundPaths { get; set; } = new List<WadSoundPath>
            {
                new WadSoundPath("Sounds"), // For directly loading wad files.
                new WadSoundPath("Sound"),
                new WadSoundPath("Sounds" + Dir + "Samples"),
                new WadSoundPath("Sound" + Dir + "Samples"),
                new WadSoundPath(".." + Dir + "Sounds"), // For directly loading wad files.
                new WadSoundPath(".." + Dir + "Sound"),
                new WadSoundPath(".." + Dir + "Sounds" + Dir + "Samples"),
                new WadSoundPath(".." + Dir + "Sound" + Dir + "Samples"),
                new WadSoundPath(".." + Dir + ".." + Dir + "Sounds"), // For directly loading wad files.
                new WadSoundPath(".." + Dir + ".." + Dir + "Sound"),
                new WadSoundPath(".." + Dir + ".." + Dir + "Sounds" + Dir + "Samples"),
                new WadSoundPath(".." + Dir + ".." + Dir + "Sound" + Dir + "Samples"),
                new WadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + "Sound" + Dir + "Samples"),
                new WadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + ".." + Dir + "Sound" + Dir + "Samples"),
                new WadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + ".." + Dir + ".." + Dir + "Sound" + Dir + "Samples"),
                new WadSoundPath(VariableCreate(VariableType.GameDirectory) + Dir + "Sound" + Dir + "Samples"),
                new WadSoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds" + Dir + VariableCreate(VariableType.SoundEngineVersion) + Dir + "Samples"),
                new WadSoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds" + Dir + "Samples")
            };

        // Game version and version-specific settings
        public TRVersion.Game GameVersion { get; set; } = TRVersion.Game.TR4;
        public bool GameEnableQuickStartFeature { get; set; } = true;

        // Paths
        public string LevelFilePath { get; set; } = null; // Can be null if the level has not been loaded from / saved to disk yet.
        public string FontTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string SkyTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string Tr5ExtraSpritesFilePath { get; set; } = null; // Can be null if the default should be used.
        public string ScriptDirectory { get; set; } = VariableCreate(VariableType.EditorDirectory) + Dir + "Script";
        public string GameDirectory { get; set; } = VariableCreate(VariableType.EditorDirectory) + Dir + "Game";
        public string GameLevelFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "data" + Dir + VariableCreate(VariableType.LevelName) + ".tr4"; // Relative to "GameDirectory"
        public string GameExecutableFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "Tomb4.exe"; // Relative to "GameDirectory"

        // All data lists
        public List<ReferencedWad> Wads { get; set; } = new List<ReferencedWad>();
        public List<LevelTexture> Textures { get; set; } = new List<LevelTexture>();
        public List<ReferencedSoundsCatalog> SoundsCatalogs { get; set; } = new List<ReferencedSoundsCatalog>();
        public List<ImportedGeometry> ImportedGeometries { get; set; } = new List<ImportedGeometry>();
        public List<ImportedGeometry> ImportedRooms { get; set; } = new List<ImportedGeometry>();
        public bool InterpretStaticMeshVertexDataForMerge { get; set; } = false;
        public List<AutoStaticMeshMergeEntry> AutoStaticMeshMerges { get; set; } = new List<AutoStaticMeshMergeEntry>();
        public List<AnimatedTextureSet> AnimatedTextureSets { get; set; } = new List<AnimatedTextureSet>();
        public List<ColorC> Palette { get; set; } = LoadPalette(ResourcesC.ResourcesC.palette);

        // Light options
        public Vector3 DefaultAmbientLight { get; set; } = new Vector3(0.25f, 0.25f, 0.25f);
        public LightQuality DefaultLightQuality { get; set; } = LightQuality.Low;
        public bool OverrideIndividualLightQualitySettings { get; set; } = false;

        // Compiler options
        public bool AgressiveFloordataPacking { get; set; } = false;
        public bool AgressiveTexturePacking { get; set; } = false;
        public bool Dither16BitTextures { get; set; } = true;
        public bool RemapAnimatedTextures { get; set; } = true;
        public int TexturePadding { get; set; } = 8;
        public bool FastMode { get; set; } = false;

        // For TR5 only
        public Tr5LaraType Tr5LaraType { get; set; } = Tr5LaraType.Normal;
        public Tr5WeatherType Tr5WeatherType { get; set; } = Tr5WeatherType.Normal;

        public LevelSettings Clone()
        {
            LevelSettings result = (LevelSettings)MemberwiseClone();
            result.Wads = Wads.ConvertAll(wad => wad.Clone());
            result.WadSoundPaths = WadSoundPaths.ConvertAll(soundPath => soundPath.Clone());
            result.SoundsCatalogs = SoundsCatalogs.ConvertAll(catalog => catalog.Clone());
            result.Textures = Textures.ConvertAll(texture => (LevelTexture)texture.Clone());
            result.AnimatedTextureSets = AnimatedTextureSets.ConvertAll(set => set.Clone());
            result.ImportedGeometries = ImportedGeometries.ConvertAll(geometry => geometry.Clone());
            result.AutoStaticMeshMerges = AutoStaticMeshMerges.ConvertAll(entry => entry.Clone());
            return result;
        }

        object ICloneable.Clone() => Clone();

        public static string VariableCreate(VariableType type)
        {
            return VariableBegin + type + VariableEnd;
        }

        public void ConvertLevelExtension()
        {
            var result = string.Empty;
            switch (GameVersion)
            {
                case TRVersion.Game.TR1:
                    result = ".phd";
                    break;
                case TRVersion.Game.TR2:
                case TRVersion.Game.TR3:
                    result = ".tr2";
                    break;
                case TRVersion.Game.TR4:
                case TRVersion.Game.TRNG:
                default:
                    result = ".tr4";
                    break;
                case TRVersion.Game.TR5:
                    result = ".trc";
                    break;
                case TRVersion.Game.TR5Main:
                    result = ".t5m";
                    break;
            }
            GameLevelFilePath = Path.ChangeExtension(GameLevelFilePath, result);
        }

        public string GetVariable(VariableType type)
        {
            string result;

            switch (type)
            {
                case VariableType.EditorDirectory:
                    result = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    break;
                case VariableType.ScriptDirectory:
                    result = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Dir + "Script";
                    break;
                case VariableType.LevelDirectory:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                        result = Path.GetDirectoryName(LevelFilePath);
                    else
                        result = GetVariable(VariableType.EditorDirectory);
                    break;
                case VariableType.GameDirectory:
                    result = MakeAbsolute(GameDirectory ?? VariableCreate(VariableType.LevelDirectory), VariableType.GameDirectory);
                    break;
                case VariableType.LevelName:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                        result = PathC.GetFileNameWithoutExtensionTry(LevelFilePath);
                    else if (Wads.Count > 0 && !string.IsNullOrEmpty(Wads[0].Path))
                        result = PathC.GetFileNameWithoutExtensionTry(Wads[0].Path);
                    else
                        result = "Default";
                    break;
                case VariableType.EngineVersion:
                    result = GameVersion.ToString();
                    break;
                case VariableType.SoundEngineVersion:
                    result = (GameVersion.Native()).ToString();
                    break;
                default:
                    throw new ArgumentException();
            }
            if (result == null) result = "";
            return result;
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

        public ImageC LoadFontTexture(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.Font_pc);
            return LoadRawExtraTexture(path);
        }

        public ImageC LoadSkyTexture(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.pcsky_raw);
            return LoadRawExtraTexture(path);
        }

        public ImageC LoadTr5ExtraSprites(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.Extra_Tr5_pc);
            return LoadRawExtraTexture(path);
        }

        public static List<ColorC> LoadPalette(byte[] buffer)
        {
            var result = new List<ColorC>();
            if (buffer.Length < 3) return result; // No suitable color data found

            using (var stream = new MemoryStream(buffer, false))
            using (var readerPalette = new BinaryReader(stream))
                while (readerPalette.BaseStream.Position < readerPalette.BaseStream.Length)
                    result.Add(new ColorC(readerPalette.ReadByte(), readerPalette.ReadByte(), readerPalette.ReadByte()));

            return result;
        }
        public static List<ColorC> LoadPalette() => LoadPalette(ResourcesC.ResourcesC.palette);

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

            // Add other imported geometry textures to lookup
            foreach (ImportedGeometry importedGeometry in ImportedGeometries)
                foreach (ImportedGeometryTexture importedGeometryTexture in importedGeometry.Textures)
                    if (!absolutePathTextureLookup.ContainsKey(importedGeometryTexture.AbsolutePath))
                        absolutePathTextureLookup.Add(importedGeometryTexture.AbsolutePath, importedGeometryTexture);

            // TODO Ideally we could load these concurrently
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

        public SortedList<WadMoveableId, WadMoveable> WadGetAllMoveables()
        {
            SortedList<WadMoveableId, WadMoveable> result = new SortedList<WadMoveableId, WadMoveable>();
            foreach (ReferencedWad wad in Wads)
                if (wad.Wad != null)
                    foreach (KeyValuePair<WadMoveableId, WadMoveable> moveable in wad.Wad.Moveables)
                        if (!result.ContainsKey(moveable.Key))
                            result.Add(moveable.Key, moveable.Value);
            return result;
        }

        public SortedList<WadStaticId, WadStatic> WadGetAllStatics()
        {
            SortedList<WadStaticId, WadStatic> result = new SortedList<WadStaticId, WadStatic>();
            foreach (ReferencedWad wad in Wads)
                if (wad.Wad != null)
                    foreach (KeyValuePair<WadStaticId, WadStatic> @static in wad.Wad.Statics)
                        if (!result.ContainsKey(@static.Key))
                            result.Add(@static.Key, @static.Value);
            return result;
        }

        public SortedList<WadSpriteSequenceId, WadSpriteSequence> WadGetAllSpriteSequences()
        {
            SortedList<WadSpriteSequenceId, WadSpriteSequence> result = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
            foreach (ReferencedWad wad in Wads)
                if (wad.Wad != null)
                    foreach (KeyValuePair<WadSpriteSequenceId, WadSpriteSequence> sprite in wad.Wad.SpriteSequences)
                        if (!result.ContainsKey(sprite.Key))
                            result.Add(sprite.Key, sprite.Value);
            return result;
        }

        public List<WadSprite> WadGetAllSprites() => WadGetAllSpriteSequences().Values.SelectMany(s => s.Sprites).ToList();

        public static IEnumerable<FileFormat> FileFormatsLoadRawExtraTexture =>
            new[] { new FileFormat("Raw sky/font image", "raw", "pc") }.Concat(ImageC.FromFileFileExtensions);
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevel = new[] { new FileFormat("Tomb Editor Level", "prj2") };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevelPrj = new[] { new FileFormat("Tomb Editor Level", "prj") };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevelCompiled = new[]
        {
            new FileFormat("Tomb Raider I level", "phd"),
            new FileFormat("Tomb Raider II/III level", "tr2"),
            new FileFormat("Tomb Raider The Last Revelation level", "tr4"),
            new FileFormat("Tomb Raider Chronicles level", "trc"),
            new FileFormat("TR5Main level", "t5m")
        };

        public static readonly IReadOnlyCollection<FileFormat> FileFormatsSoundsCatalogs = new[]
        {
            new FileFormat("TRLE sound catalog", "txt"),
            new FileFormat("Tomb Editor sound catalog", "xml")
        };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsSoundsXmlFiles = new[] { new FileFormat("XML file", "xml") };

        public WadSoundInfo WadTryGetSoundInfo(int id)
        {
            foreach (var soundInfo in GlobalSoundMap)
                if (soundInfo.Id == id)
                    return soundInfo;
            return null;
        }

        public bool AutoStaticMeshMergeContainsStaticMesh(WadStatic staticMesh) => AutoStaticMeshMerges.Where(e => e.meshId == staticMesh.Id.TypeId).Any();
        public AutoStaticMeshMergeEntry GetStaticMergeEntry(WadStaticId staticMeshId) => AutoStaticMeshMerges.FirstOrDefault(e => e.meshId == staticMeshId.TypeId);

        public List<int> SelectedAndAvailableSounds
        { get { return SelectedSounds.Where(item => GlobalSoundMap
                .Any(entry => entry.Id == item && entry.Samples.Count > 0 && entry.SampleCount(this) > 0)).ToList(); } }

        public List<int> SelectedAndMissingSounds
        { get { return SelectedSounds.Where(item => !GlobalSoundMap.Any(entry => entry.Id == item)).ToList(); } }
    }
}
