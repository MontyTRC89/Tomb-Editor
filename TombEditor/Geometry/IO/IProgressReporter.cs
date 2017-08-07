using System.Windows.Forms;

namespace TombEditor.Geometry.IO
{
    public interface IProgressReporter : IWin32Window
    {
        void ReportProgress(int progress, string message);
    }
}
