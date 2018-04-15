using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public class ReferencedWad
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class UniqueIDType { }
        public UniqueIDType UniqueID { get; private set; } = new UniqueIDType();
        public static IReadOnlyList<FileFormat> FileExtensions => Wad2.WadFormatExtensions;

        public string Path { get; private set; }
        public Wad2 Wad { get; private set; }
        public Exception LoadException { get; private set; }

        public ReferencedWad()
        {
            Path = null;
            Reload(null);
        }

        public ReferencedWad(LevelSettings settings, string path, IDialogHandler progressReporter = null)
        {
            Path = path;
            Reload(settings);
        }

        public void Reload(LevelSettings settings, IDialogHandler progressReporter = null)
        {
            if (string.IsNullOrEmpty(Path))
            {
                Wad = null;
                LoadException = new Exception("Path is empty.");
                return;
            }

            // Load wad
            try
            {
                Wad2 newWad = Wad2.ImportFromFile(
                    settings.MakeAbsolute(Path),
                    settings.OldWadSoundPaths.Select(soundPath => settings.ParseVariables(soundPath.Path)),
                    progressReporter ?? new ProgressReporterSimple());
                Wad = newWad;
                LoadException = null;
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to load wad '" + Path + "'.");
                Wad = null;
                LoadException = exc;
            }
        }

        public void SetPath(LevelSettings settings, string path)
        {
            Path = path;
            Reload(settings);
        }
    }
}
