using DarkUI.Forms;
using NLog;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

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
                    });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "PRJ loading failed");

                string message = "There was an error. Message: " + ex.Message;
                this?.Invoke((Action)delegate
                    {
                        pbStato.Value = 0;

                        if (!string.IsNullOrEmpty(message))
                        {
                            lstLog.Items.Add(message);
                            lstLog.SelectedIndex = lstLog.Items.Count - 1;
                            lstLog.TopIndex = lstLog.Items.Count - 1;
                        }

                        lstLog.BackColor = Color.LightPink;
                        butCancel.Focus();
                    });
            }
        }
        
        void IProgressReporter.ReportProgress(float progress, string message)
        {
            logger.Info(progress.ToString() + " - " + message);

            if (!(bool)this?.Invoke((Func<bool>)delegate
                {
                    pbStato.Value = (int)Math.Round(progress, 0);

                    if (!string.IsNullOrEmpty(message))
                    {
                        lstLog.Items.Add(message);
                        lstLog.SelectedIndex = lstLog.Items.Count - 1;
                        lstLog.TopIndex = lstLog.Items.Count - 1;
                    }
                    
                    return !_threadShouldAbort;
                }))
                throw new OperationCanceledException();
        }

        void IProgressReporter.InvokeGui(Action<IWin32Window> action)
        {
            if (InvokeRequired)
                Invoke(action, this);
            else
                action(this);
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
            lstLog.Items.Add("Trying to stop the *.prj import...");
            for (int i = 0; i < 23; ++i)
            {
                if (_thread.Join(40))
                    return;
                Application.DoEvents();
            }

            // Shut the thread down forcefully
            lstLog.Items.Add("Forcefully stopping *.prj import.");
            _thread.Abort();
            _thread.Join();
        }
    }
}
