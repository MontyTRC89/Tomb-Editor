#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using NLog;
using TombLib;
using TombLib.Forms;
using TombLib.Utils;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Operation;

public partial class OperationDialogWindowViewModel : ObservableObject, IModalDialogViewModel, IProgressReporter
{
	public enum LogSeverity { Info, Warning, Error }

	public sealed class LogEntry
	{
		public string Text { get; init; } = string.Empty;
		public LogSeverity Severity { get; init; } = LogSeverity.Info;
	}

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	private readonly Action<IProgressReporter, CancellationToken> _operation;
	private readonly bool _autoCloseWhenDone;
	private readonly CancellationTokenSource _cts = new();
	private readonly Dispatcher _dispatcher;

	private Task? _task;
	private IntPtr _hwnd = IntPtr.Zero;
	private IntPtr _taskbarHwnd = IntPtr.Zero;

	[ObservableProperty] private bool? _dialogResult;
	[ObservableProperty] private string _title = string.Empty;
	[ObservableProperty] private bool _isProgressVisible = true;
	[ObservableProperty] private int _progress;
	[ObservableProperty] private bool _isOkEnabled;
	[ObservableProperty] private bool _isCancelEnabled = true;

	public ObservableCollection<LogEntry> LogEntries { get; } = new();

	public OperationDialogWindowViewModel(
		string operationName,
		bool autoCloseWhenDone,
		bool noProgressBar,
		Action<IProgressReporter, CancellationToken> operation,
		ILocalizationService? localizationService = null)
	{
		Title = operationName;
		_autoCloseWhenDone = autoCloseWhenDone;
		IsProgressVisible = !noProgressBar;
		_operation = operation;
		_ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);
		_dispatcher = Dispatcher.CurrentDispatcher;
	}

	/// <summary>Provided by the View on Loaded; owns sub-dialogs raised during the operation.</summary>
	public void SetWindowHandle(IntPtr hwnd) => _hwnd = hwnd;

	/// <summary>
	/// The window whose taskbar button reflects build/open progress. The operation dialog itself is
	/// hidden from the taskbar (ShowInTaskbar=False), so progress must target the owning main window.
	/// </summary>
	public void SetTaskbarWindowHandle(IntPtr hwnd) => _taskbarHwnd = hwnd;

	public async void Start()
	{
		try
		{
			IsCancelEnabled = true;
			IsOkEnabled = false;

			if (IsProgressVisible && _taskbarHwnd != IntPtr.Zero)
				TaskbarProgress.SetState(_taskbarHwnd, TaskbarProgress.TaskbarStates.Normal);

			_task = Task.Run(() =>
			{
				_operation(this, _cts.Token);
				_cts.Token.ThrowIfCancellationRequested();
			});

			await _task;
			OnSuccess();
		}
		catch (Exception ex)
		{
			OnFailure(ex);
			_logger.Error(ex, "Operation failed: " + Title);

			string message = "There was an error. Message: " + ex.Message;
			if (ex.InnerException is not null)
				message += " : " + ex.InnerException.Message;

			AppendLine(message, LogSeverity.Error);
#if DEBUG
			throw;
#endif
		}
	}

	private void OnFailure(Exception ex)
	{
		if (_taskbarHwnd != IntPtr.Zero)
		{
			if (ex is not OperationCanceledException)
				TaskbarProgress.SetState(_taskbarHwnd, TaskbarProgress.TaskbarStates.Error);
			else
				TaskbarProgress.SetState(_taskbarHwnd, TaskbarProgress.TaskbarStates.NoProgress);
			TaskbarProgress.FlashWindow(_taskbarHwnd);
		}

		Progress = 0;
		IsCancelEnabled = true;
		IsOkEnabled = false;
	}

	private void OnSuccess()
	{
		if (_taskbarHwnd != IntPtr.Zero)
		{
			TaskbarProgress.SetState(_taskbarHwnd, TaskbarProgress.TaskbarStates.NoProgress);
			TaskbarProgress.FlashWindow(_taskbarHwnd);
		}

		Progress = 100;
		IsOkEnabled = true;
		IsCancelEnabled = false;

		if (_autoCloseWhenDone)
			DialogResult = true;
	}

	private void AddMessage(float? progress, string message, bool isWarning)
	{
		if (_cts.IsCancellationRequested)
			return;

		_dispatcher.BeginInvoke(() =>
		{
			if (progress.HasValue)
			{
				Progress = (int)Math.Round(MathC.Clamp(progress.Value, 0, 100), 0);
				if (_taskbarHwnd != IntPtr.Zero)
					TaskbarProgress.SetValue(_taskbarHwnd, progress.Value, 100);
			}

			if (!string.IsNullOrEmpty(message))
				AppendLine(message, isWarning ? LogSeverity.Warning : LogSeverity.Info);
		});
	}

	private void AppendLine(string message, LogSeverity severity)
		=> LogEntries.Add(new LogEntry { Text = message, Severity = severity });

	void IProgressReporter.ReportWarn(string message)
	{
		_logger.Warn(message);
		AddMessage(null, message, true);
	}

	void IProgressReporter.ReportInfo(string message)
	{
		_logger.Info(message);
		AddMessage(null, message, false);
	}

	void IProgressReporter.ReportProgress(float progress, string message)
	{
		_logger.Info(progress + " - " + message);
		AddMessage(progress, message, false);
	}

	void IDialogHandler.RaiseDialog(IDialogDescription description)
	{
		// GraphicalDialogHandler needs an IWin32Window owner. Wrap the WPF HWND.
		var owner = _hwnd != IntPtr.Zero ? (IWin32Window)new HwndOwner(_hwnd) : null!;
		GraphicalDialogHandler.HandleDialog(description, owner);
	}

	[RelayCommand]
	private void Ok() => DialogResult = true;

	[RelayCommand]
	private void Cancel()
	{
		if (_task is not null && _task.Status >= TaskStatus.RanToCompletion)
		{
			DialogResult = false;
			return;
		}
		EndThread();
	}

	private void EndThread()
	{
		_cts.Cancel();
		DialogResult = false;
		AppendLine("Stopping the process...", LogSeverity.Warning);
	}

	/// <summary>Called by the View when the user attempts to close while running.</summary>
	public bool TryRequestClose()
	{
		if (_task is null)
			return true;

		if (_task.Status <= TaskStatus.Running)
		{
			if (!_cts.IsCancellationRequested)
				EndThread();
			return false;
		}

		if (_taskbarHwnd != IntPtr.Zero)
			TaskbarProgress.SetState(_taskbarHwnd, TaskbarProgress.TaskbarStates.NoProgress);
		return true;
	}

	private sealed class HwndOwner : IWin32Window
	{
		public HwndOwner(IntPtr h) { Handle = h; }
		public IntPtr Handle { get; }
	}
}
