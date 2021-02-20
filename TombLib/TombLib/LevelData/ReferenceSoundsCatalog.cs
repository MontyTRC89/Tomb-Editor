using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public class ReferencedSoundsCatalog : ICloneable, IEquatable<ReferencedSoundsCatalog>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class UniqueIDType { }
        public UniqueIDType UniqueID { get; private set; } = new UniqueIDType();
        public static IReadOnlyList<FileFormat> FileExtensions => WadSounds.FormatExtensions;

        public string Path { get; private set; }
        public WadSounds Sounds { get; private set; }
        public Exception LoadException { get; private set; }

        public ReferencedSoundsCatalog()
        {
            Path = null;
            Reload(null);
        }

        public ReferencedSoundsCatalog(LevelSettings settings, string path, IDialogHandler progressReporter = null)
        {
            Path = path;
            Reload(settings, progressReporter);
        }

        public void Reload(LevelSettings settings, IDialogHandler progressReporter = null)
        {
            if (string.IsNullOrEmpty(Path))
            {
                Sounds = null;
                LoadException = new Exception("Path is empty.");
                return;
            }

            // Load the catalog
            try
            {
                var path = settings.MakeAbsolute(Path);
                if (File.Exists(path))
                {
                    WadSounds newSounds = WadSounds.ReadFromFile(path);
                    Sounds = newSounds;
                    LoadException = null;
                }
                else
                    LoadException = new Exception("File not found: " + path);
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to load catalog '" + Path + "'.");
                Sounds = null;
                LoadException = exc;
            }
        }

        public void SetPath(LevelSettings settings, string path)
        {
            Path = path;
            Reload(settings);
        }

        public void Assign(ReferencedSoundsCatalog other)
        {
            Path = other.Path;
            Sounds = other.Sounds;
            LoadException = other.LoadException;
        }

        public ReferencedSoundsCatalog Clone() => (ReferencedSoundsCatalog)MemberwiseClone(); // Don't copy the data pointer
        object ICloneable.Clone() => Clone();

        public bool Equals(ReferencedSoundsCatalog other)
        {
            if (other == null) return false;
            return (UniqueID == other.UniqueID && Path?.ToLower() == other.Path?.ToLower());
        }
    }
}
