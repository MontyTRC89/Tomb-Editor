using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace TombEditor.Geometry
{
    public enum VariableType
    {
        [Description("The directory of the *.prj2 file.")]
        LevelDirectory,
        [Description("The directory in which all game components reside.")]
        GameDirectory,
        [Description("The directory of the editor application.")]
        EditorDirectory,
        [Description("The name of the *.prj2 file.")]
        LevelName,
        None,
    }

    public class SoundPath : ICloneable
    {
        public string Path { get; set; }

        public SoundPath(string path)
        {
            Path = path;
        }

        public SoundPath Clone()
        {
            return (SoundPath)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public class LevelSettings : ICloneable
    {
        public const string VariableBegin = "$(";
        public const string VariableEnd = ")";
        public static readonly char Dir = Path.DirectorySeparatorChar;

        public string LevelFilePath { get; set; } = null; // Can be null if the level has not been loaded from / saved to disk yet.  
        public string TextureFilePath { get; set; } = null; // Can be null if no texture file is loaded.
        public string WadFilePath { get; set; } = null; // Can be null if no object file is loaded.
        public string FontTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string SkyTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public List<SoundPath> SoundPaths { get; set; } = new List<SoundPath>
            {
                new SoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + "Sounds"),
                new SoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds")
            };

        public string GameDirectory { get; set; } = VariableCreate(VariableType.EditorDirectory) + Dir + "Game";
        public string GameLevelFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "data" + Dir + VariableCreate(VariableType.LevelName) + ".tr4"; // Relative to "GameDirectory"
        public string GameExecutableFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "Tomb4.exe"; // Relative to "GameDirectory"
        public bool IgnoreMissingSounds { get; set; } = false;

        
        public LevelSettings Clone()
        {
            LevelSettings result = (LevelSettings)MemberwiseClone();
            result.SoundPaths = SoundPaths.ConvertAll((soundPath) => soundPath.Clone());
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

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
                    return System.Windows.Forms.Application.StartupPath;
                case VariableType.LevelDirectory:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                        return Path.GetDirectoryName(LevelFilePath);
                    return GetVariable(VariableType.EditorDirectory);
                case VariableType.GameDirectory:
                    return MakeAbsolute(GameDirectory ?? VariableCreate(VariableType.LevelDirectory), VariableType.GameDirectory);
                case VariableType.LevelName:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                        return Path.GetFileNameWithoutExtension(LevelFilePath);
                    return "";
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
                if ((!Enum.TryParse<VariableType>(variableName, out variableType)) ||
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
                        string relativePath = Utils.GetRelativePath(GetVariable(baseDirType), path);
                        if (relativePath == path)
                            return path;
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
            Path.Combine(System.Windows.Forms.Application.StartupPath, "Editor/Font.pc.png");

        public string SkyTextureFileNameAbsoluteOrDefault => MakeAbsolute(SkyTextureFilePath) ??
            Path.Combine(System.Windows.Forms.Application.StartupPath, "Editor/pcsky.raw.png");

        public string LookupSound(string soundName, bool ignoreMissingSounds)
        {
            foreach (var soundPath in SoundPaths)
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
    }
}
