using System;
using System.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public abstract class LevelCompiler : IDisposable
    {
        protected readonly Level _level;
        protected readonly string _dest;
        protected readonly string _backup;
        protected readonly IProgressReporter _progressReporter;

        protected bool _compiledSuccessfully;

        protected LevelCompiler(Level level, string dest, IProgressReporter progressReporter)
        {
            _level = level;
            _backup = dest;
            _dest = Path.GetDirectoryName(dest) + "\\" + Path.GetFileNameWithoutExtension(dest) + ".tmp";
            _progressReporter = progressReporter;

            _compiledSuccessfully = false;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Delete old backup and replace it with new level file
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
    }
}
