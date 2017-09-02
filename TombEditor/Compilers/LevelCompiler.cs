using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public abstract class LevelCompiler
    {
        protected readonly Level Level;
        protected readonly string Dest;
        protected readonly IProgressReporter ProgressReporter;

        protected LevelCompiler(Level level, IProgressReporter progressReporter)
        {
            Level = level;
            Dest = Level.Settings.MakeAbsolute(Level.Settings.GameLevelFilePath);
            ProgressReporter = progressReporter;
        }

        protected void ReportProgress(float percentage, string message)
        {
            ProgressReporter.ReportProgress(percentage, message);
        }
    }
}
