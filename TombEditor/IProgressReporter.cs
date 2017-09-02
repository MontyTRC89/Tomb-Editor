using NLog;
using System;
using System.Windows.Forms;

namespace TombEditor
{
    public interface IProgressReporter
    {
        void ReportWarn(string message);
        void ReportInfo(string message);
        void ReportProgress(float progress, string message);
        void InvokeGui(Action<IWin32Window> action);
    }

    public struct ProgressReporterSimple : IProgressReporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public IWin32Window Owner { get; }

        public ProgressReporterSimple(IWin32Window owner)
        {
            Owner = owner;
        }
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
            logger.Info($"{progress} - {message}");
        }
        public void InvokeGui(Action<IWin32Window> action)
        {
            action(Owner);
        }
    }
}
