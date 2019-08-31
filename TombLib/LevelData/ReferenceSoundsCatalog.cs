using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public class ReferencedSoundsCatalog : ICloneable
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
                WadSounds newSounds = WadSounds.ReadFromFile(settings.MakeAbsolute(Path));
                /*Wad2 newWad = Wad2.ImportFromFile(
                    settings.MakeAbsolute(Path),
                    settings.OldWadSoundPaths.Select(soundPath => settings.ParseVariables(soundPath.Path)),
                    progressReporter ?? new ProgressReporterSimple());*/
                Sounds = newSounds;
                LoadException = null;
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
    }
}
