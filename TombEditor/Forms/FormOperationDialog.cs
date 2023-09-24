using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DarkUI.Forms;
using NLog;
using TombLib.Forms;
using TombLib.Utils;
using TombLib;
using DarkUI.Extensions;
using DarkUI.Config;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TombEditor.Forms
{
	public partial class FormOperationDialog : DarkForm, IProgressReporter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private readonly Action<IProgressReporter, CancellationToken> _operation;
		private readonly bool _autoCloseWhenDone;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private Task _task;
		public FormOperationDialog(string operationName, bool autoCloseWhenDone, bool noProgressBar, Action<IProgressReporter, CancellationToken> operation)
		{
			_autoCloseWhenDone = autoCloseWhenDone;
			_operation = operation;
			InitializeComponent();

			Text = operationName;
			lstLog.SelectionHangingIndent = 50;

			this.SetActualSize();

			panelProgressBar.Visible = !noProgressBar;
			if (!noProgressBar && Application.OpenForms.Count > 0)
				TaskbarProgress.SetState(Application.OpenForms[0].Handle, TaskbarProgress.TaskbarStates.Normal);

			lstLog.BackColor = lstLog.BackColor.Multiply(Colors.Brightness);
			_cancellationTokenSource = new CancellationTokenSource();

		}

		private void FormOperationDialog_Shown(object sender, EventArgs e)
		{
			Run();
		}

		private void FormOperationDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_task != null)
			{
				e.Cancel = true;
				if (e.CloseReason == CloseReason.UserClosing && !_cancellationTokenSource.IsCancellationRequested)
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
				var task = Task.Run(() =>
				{
					_operation(this, _cancellationTokenSource.Token);
				});
				// Done

				task.ContinueWith((t) =>
				{
					BeginInvoke(() =>
					{
						TaskbarProgress.SetState(Application.OpenForms[0].Handle, TaskbarProgress.TaskbarStates.NoProgress);
						TaskbarProgress.FlashWindow();

						pbStato.Value = 100;

						butOk.Enabled = true;
						butCancel.Enabled = false;
						butOk.Focus();

						if (_autoCloseWhenDone)
						{
							DialogResult = DialogResult.OK;
							Close();
						}
						lstLog.BackColor = Color.LightGreen.Multiply(Colors.Brightness);
					});
				});

				
#if !DEBUG
			}
			catch (Exception ex)
			{
				while (ex is AggregateException) // Improve error messages from exceptions of Task.* threads
					ex = ex.InnerException;

				logger.Error(ex, "Operation failed: " + Text);

				string message = "There was an error. Message: " + ex.Message;
				Invoke(() =>
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
			BeginInvoke(() =>
			{
				if (progress.HasValue)
				{
					pbStato.Value = (int)Math.Round(MathC.Clamp(progress.Value, 0, 100), 0);
					TaskbarProgress.SetValue(Application.OpenForms[0].Handle, progress.Value, 100);
				}

				if (!string.IsNullOrEmpty(message))
				{
					lstLog.SelectionBackColor = isWarning ? Color.Yellow.Multiply(Colors.Brightness) : Color.Empty;
					lstLog.AppendText(message + "\n");
					lstLog.ScrollToCaret();
				}
			});
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
			DialogResult = DialogResult.OK;
			Close();
		}

		private void EndThread()
		{
			Invoke(() =>
			{
				DialogResult = DialogResult.Cancel;
				_cancellationTokenSource.Cancel();
				// Try shutting down the thread softly
				lstLog.SelectionBackColor = Color.Tomato;
				lstLog.AppendText("Stopping the process...\n");
				lstLog.ScrollToCaret();
			});

		}
	}
}
