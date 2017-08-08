using NLog;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TombEditor
{
    public interface IProgressReporter
    {
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
        public void ReportProgress(float progress, string message)
        {
            logger.Info(progress.ToString() + " - " + message);
        }
        public void InvokeGui(Action<IWin32Window> action)
        {
            action(Owner);
        }
    }
}
