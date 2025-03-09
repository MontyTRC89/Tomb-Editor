using System;
using System.IO;
using System.Threading;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public class CompilerStatistics
    {
        public int BoxCount { get; set; }
        public int OverlapCount { get; set; }
        public int ObjectTextureCount { get; set; }
        public override string ToString()
        {
            return "Boxes: " + BoxCount + " | Overlaps: " + OverlapCount + " | TexInfos: " + ObjectTextureCount;
        }
    }

    public abstract class LevelCompiler : IDisposable
    {
        protected readonly Level _level;
        protected readonly string _dest;
        protected readonly string _finalDest;
        protected readonly string _backup;
        protected readonly IProgressReporter _progressReporter;

        protected bool _compiledSuccessfully;

        protected LevelCompiler(Level level, string dest, IProgressReporter progressReporter)
        {
            _level = level;
            _backup = dest;
            _finalDest = dest;
            _dest = Path.GetDirectoryName(dest) + "\\" + Path.GetFileNameWithoutExtension(dest) + ".tmp";
            _progressReporter = progressReporter;

            _compiledSuccessfully = false;
        }

        public void Dispose()
        {
            if (_compiledSuccessfully)
            {
                if (File.Exists(_backup))
                    File.Delete(_backup);
                File.Move(_dest, _backup);
            }
            else
            {
                if (File.Exists(_dest))
                    File.Delete(_dest);
            }
        }

        protected void ReportProgress(float percentage, string message)
        {
            _progressReporter.ReportProgress(percentage, message);
        }

        public abstract CompilerStatistics CompileLevel(CancellationToken cancelToken);
    }
}
