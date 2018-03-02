using DarkUI.Extensions;
using DarkUI.Forms;
using NLog;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Utils;

namespace TombEditor
{
    public partial class FormOperationDialog : DarkForm, IProgressReporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Action<IProgressReporter> _operation;
        private Thread _thread;
        private volatile bool _threadShouldAbort = false;
        private bool _autoCloseWhenDone = false;

        public FormOperationDialog(string operationName, bool autoCloseWhenDone, Action<IProgressReporter> operation)
        {
            _autoCloseWhenDone = autoCloseWhenDone;
            _operation = operation;
            InitializeComponent();

            Text = operationName;
            lstLog.SelectionHangingIndent = 50;

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(152, 93) + (Size - ClientSize);
        }

        private void FormBuildLevel_Shown(object sender, EventArgs e)
        {
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void FormImportPRJ_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) && _thread.IsAlive && !_threadShouldAbort)
            {
                EndThread();
                e.Cancel = true;
            }
        }

        private void Run()
        {
            try
            {
                _operation(this);

                // Done
                this?.BeginInvoke((Action)delegate
                    {
                        pbStato.Value = 100;
                        butOk.Enabled = true;
                        butOk.Focus();

                        if (_autoCloseWhenDone)
                        {
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        lstLog.BackColor = Color.LightGreen;
                    });
            }
            catch (Exception ex)
            {
                while (ex is AggregateException) // Improve error messages from exceptions of Task.* threads
                    ex = ex.InnerException;

                logger.Error(ex, "PRJ loading failed");

                string message = "There was an error. Message: " + ex.Message;
                this?.Invoke((Action)delegate
                    {
                        pbStato.Value = 0;

                        if (!string.IsNullOrEmpty(message))
                        {
                            lstLog.SelectionBackColor = Color.Tomato;
                            lstLog.AppendText(message + "\n");
                        }

                        lstLog.BackColor = Color.LightPink;
                        butCancel.Focus();
                    });
            }
        }

        void AddMessage(float? progress, string message, bool isWarning)
        {
            if (!(bool)this?.Invoke((Func<bool>)delegate
            {
                if (progress.HasValue)
                    pbStato.SetProgressNoAnimation((int)Math.Round(progress.Value, 0));

                if (!string.IsNullOrEmpty(message))
                {
                    lstLog.SelectionBackColor = isWarning ? Color.Yellow : Color.Empty;
                    lstLog.AppendText(message + "\n");
                }

                return !_threadShouldAbort;
            }))
                throw new OperationCanceledException();
        }

        void IProgressReporter.ReportWarn(string message)
        {
            logger.Warn(message);
            AddMessage(null, message, true);

        }

        void IProgressReporter.ReportInfo(string message)
        {
            logger.Info(message);
            AddMessage(null, message, false);
        }

        void IProgressReporter.ReportProgress(float progress, string message)
        {
            logger.Info(progress.ToString() + " - " + message);
            AddMessage(progress, message, false);
        }

        void IDialogHandler.RaiseDialog(IDialogDescription description)
        {
            GraphicalDialogHandler.HandleDialog(description, this);
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            if (_thread.IsAlive && _threadShouldAbort)
                EndThread();
            else
                Close();
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            if (_thread.IsAlive)
                return;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void EndThread()
        {
            DialogResult = DialogResult.Cancel;
            _threadShouldAbort = true;

            if (!_thread.IsAlive)
                return;

            // Try shutting down the thread softly
            lstLog.SelectionBackColor = Color.Tomato;
            lstLog.AppendText("Trying to stop the process...\n");
            for (int i = 0; i < 23; ++i)
            {
                if (_thread.Join(40))
                    return;
                Application.DoEvents();
            }

            // Shut the thread down forcefully
            lstLog.SelectionBackColor = Color.Tomato;
            lstLog.AppendText("Forcefully stopping process.\n");
            _thread.Abort();
            while ((_thread.ThreadState & (ThreadState.Stopped | ThreadState.Aborted)) != 0)
                Thread.Sleep(5);
        }
    }
}
