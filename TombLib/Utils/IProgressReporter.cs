using NLog;

namespace TombLib.Utils
{
    public interface IProgressReporter : IDialogHandler
    {
        void ReportWarn(string message);
        void ReportInfo(string message);
        void ReportProgress(float progress, string message);
    }

    public class ProgressReporterSimple : IProgressReporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void ReportWarn(string message)
        {
            logger.Warn(message);
        }

        public void ReportInfo(string message)
        {
            logger.Info(message);
        }

        public void ReportProgress(float progress, string message)
        {
            logger.Info(progress.ToString() + " - " + message);
        }

        public void RaiseDialog(IDialogDescription description)
        { }
    }
}
