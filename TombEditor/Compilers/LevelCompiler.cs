using System.ComponentModel;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public abstract class LevelCompiler
    {
        protected readonly Level _level;
        protected readonly string _dest;
        protected readonly Editor _editor;
        protected readonly BackgroundWorker _worker;

        protected LevelCompiler(Level level, string dest, BackgroundWorker bw = null)
        {
            _level = level;
            _dest = dest;
            _editor = Editor.Instance;
            _worker = bw;
        }

        protected void ReportProgress(int percentage, string message)
        {
            _worker?.ReportProgress(percentage, message);
        }
    }
}
