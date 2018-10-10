using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DarkUI.Forms;
using NLog;
using TombLib.Forms;
using TombLib.Utils;
using TombLib;

namespace TombEditor.Forms
{
    public partial class FormOperationDialog : DarkForm, IProgressReporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly Action<IProgressReporter> _operation;
        private Thread _thread;
        private volatile bool _threadShouldAbort;
        private readonly bool _autoCloseWhenDone;

        public FormOperationDialog(string operationName, bool autoCloseWhenDone, bool noProgressBar, Action<IProgressReporter> operation)
        {
            _autoCloseWhenDone = autoCloseWhenDone;
            _operation = operation;
            InitializeComponent();

            Text = operationName;
            lstLog.SelectionHangingIndent = 50;

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(152, 93) + (Size - ClientSize);

            panelProgressBar.Visible = !noProgressBar;
            if(!noProgressBar)
                TaskbarProgress.SetState(Application.OpenForms[0].Handle, TaskbarProgress.TaskbarStates.Normal);
        }

        private void FormBuildLevel_Shown(object sender, EventArgs e)
        {
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void FormOperationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_thread.IsAlive)
            {
                e.Cancel = true;
                if (e.CloseReason == CloseReason.UserClosing && !_threadShouldAbort)
                {
                    butCancel.Enabled = false;
                    EndThread();
                }
            }
        }

        private void Run()
        {
#if !DEBUG
            try
            {
#endif
            _operation(this);

            // Done
            BeginInvoke((Action)delegate
                    {
                        pbStato.Value = 100;
                        TaskbarProgress.SetState(Application.OpenForms[0].Handle, TaskbarProgress.TaskbarStates.NoProgress);
                        butOk.Enabled = true;
                        butCancel.Enabled = false;
                        butOk.Focus();

                        if (_autoCloseWhenDone)
                        {
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        lstLog.BackColor = Color.LightGreen;
                    });
#if !DEBUG
            }
            catch (Exception ex)
            {
                while (ex is AggregateException) // Improve error messages from exceptions of Task.* threads
                    ex = ex.InnerException;

                logger.Error(ex, "Operation failed: " + Text);

                string message = "There was an error. Message: " + ex.Message;
                Invoke((Action)delegate
                    {
                        pbStato.Value = 0;
                        TaskbarProgress.SetState(Application.OpenForms[0].Handle, TaskbarProgress.TaskbarStates.NoProgress);

                        if (!string.IsNullOrEmpty(message))
                        {
                            lstLog.SelectionBackColor = Color.Tomato;
                            lstLog.AppendText(message + "\n");
                            lstLog.ScrollToCaret();
                        }

                        lstLog.BackColor = Color.LightPink;
                        butCancel.Enabled = true;
                        butOk.Enabled = false;
                        butCancel.Focus();
                    });
            }
#endif
        }

        void AddMessage(float? progress, string message, bool isWarning)
        {
            if (!(bool)Invoke((Func<bool>)delegate
            {
                if (progress.HasValue)
                {
                    pbStato.Value = (int)Math.Round(MathC.Clamp(progress.Value, 0, 100), 0);
                    TaskbarProgress.SetValue(Application.OpenForms[0].Handle, progress.Value, 100);
                }

                if (!string.IsNullOrEmpty(message))
                {
                    lstLog.SelectionBackColor = isWarning ? Color.Yellow : Color.Empty;
                    lstLog.AppendText(message + "\n");
                    lstLog.ScrollToCaret();
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
            logger.Info(progress + " - " + message);
            AddMessage(progress, message, false);
        }

        void IDialogHandler.RaiseDialog(IDialogDescription description)
        {
            GraphicalDialogHandler.HandleDialog(description, this);
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
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
            lstLog.ScrollToCaret();
            for (int i = 0; i < 23; ++i)
            {
                if (_thread.Join(40))
                    return;
                Application.DoEvents();
            }

            // Shut the thread down forcefully
            lstLog.SelectionBackColor = Color.Tomato;
            lstLog.AppendText("Forcefully stopping process.\n");
            lstLog.ScrollToCaret();
            _thread.Interrupt();
            while ((_thread.ThreadState & (ThreadState.Stopped | ThreadState.Aborted)) != 0)
                Thread.Sleep(5);
        }
    }
}
