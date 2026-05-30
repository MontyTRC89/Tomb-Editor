#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
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
	public sealed class LogEntry
	{
		public string Text { get; init; } = string.Empty;
		public Brush Background { get; init; } = Brushes.Transparent;
	}

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	private readonly Action<IProgressReporter, CancellationToken> _operation;
	private readonly bool _autoCloseWhenDone;
	private readonly CancellationTokenSource _cts = new();
	private readonly Dispatcher _dispatcher;

	private Task? _task;
	private IntPtr _hwnd = IntPtr.Zero;

	[ObservableProperty] private bool? _dialogResult;
	[ObservableProperty] private string _title = string.Empty;
	[ObservableProperty] private bool _isProgressVisible = true;
	[ObservableProperty] private int _progress;
	[ObservableProperty] private bool _isOkEnabled;
	[ObservableProperty] private bool _isCancelEnabled = true;
	[ObservableProperty] private Brush _logBackground = Brushes.Transparent;

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

	/// <summary>Provided by the View on Loaded so taskbar feedback targets the right window.</summary>
	public void SetWindowHandle(IntPtr hwnd) => _hwnd = hwnd;

	public async void Start()
	{
		try
		{
			IsCancelEnabled = true;
			IsOkEnabled = false;

			if (IsProgressVisible && _hwnd != IntPtr.Zero)
				TaskbarProgress.SetState(_hwnd, TaskbarProgress.TaskbarStates.Normal);

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

			AppendLine(message, Brushes.Tomato);
#if DEBUG
			throw;
#endif
		}
	}

	private void OnFailure(Exception ex)
	{
		if (_hwnd != IntPtr.Zero)
		{
			if (ex is not OperationCanceledException)
				TaskbarProgress.SetState(_hwnd, TaskbarProgress.TaskbarStates.Error);
			else
				TaskbarProgress.SetState(_hwnd, TaskbarProgress.TaskbarStates.NoProgress);
			TaskbarProgress.FlashWindow(_hwnd);
		}

		Progress = 0;
		IsCancelEnabled = true;
		IsOkEnabled = false;
		LogBackground = Brushes.LightPink;
	}

	private void OnSuccess()
	{
		if (_hwnd != IntPtr.Zero)
		{
			TaskbarProgress.SetState(_hwnd, TaskbarProgress.TaskbarStates.NoProgress);
			TaskbarProgress.FlashWindow(_hwnd);
		}

		Progress = 100;
		IsOkEnabled = true;
		IsCancelEnabled = false;
		LogBackground = Brushes.LightGreen;

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
				if (_hwnd != IntPtr.Zero)
					TaskbarProgress.SetValue(_hwnd, progress.Value, 100);
			}

			if (!string.IsNullOrEmpty(message))
				AppendLine(message, isWarning ? Brushes.Yellow : Brushes.Transparent);
		});
	}

	private void AppendLine(string message, Brush background)
		=> LogEntries.Add(new LogEntry { Text = message, Background = background });

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
		AppendLine("Stopping the process...", Brushes.Tomato);
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

		if (_hwnd != IntPtr.Zero)
			TaskbarProgress.SetState(_hwnd, TaskbarProgress.TaskbarStates.NoProgress);
		return true;
	}

	private sealed class HwndOwner : IWin32Window
	{
		public HwndOwner(IntPtr h) { Handle = h; }
		public IntPtr Handle { get; }
	}
}
