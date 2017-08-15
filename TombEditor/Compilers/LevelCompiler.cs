using System.IO;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public abstract class LevelCompiler
    {
        protected readonly Level _level;
        protected readonly string _dest;
        protected readonly Editor _editor;
        protected readonly IProgressReporter _progressReporter;

        protected LevelCompiler(Level level, string dest, IProgressReporter progressReporter)
        {
            _level = level;
            _dest = "Game\\Data\\" + Path.GetFileName(dest); // dest;
            _editor = Editor.Instance;
            _progressReporter = progressReporter;
        }

        protected void ReportProgress(float percentage, string message)
        {
            _progressReporter.ReportProgress(percentage, message);
        }
    }
}
