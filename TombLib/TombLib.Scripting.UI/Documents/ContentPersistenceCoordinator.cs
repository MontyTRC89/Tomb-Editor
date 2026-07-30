#nullable enable

using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace TombLib.Scripting.UI.Documents;

public sealed class ContentPersistenceCoordinator : IDisposable
{
	private readonly Func<string> _contentProvider;
	private readonly Func<bool> _silentSessionProvider;
	private readonly ContentChangedWorker _contentChangedWorker;
	private readonly DispatcherTimer? _textChangedDelayedTimer;
	private TimeSpan _delayedInterval = TimeSpan.FromMilliseconds(300.0);

	public ContentPersistenceCoordinator(
		Func<string> contentProvider,
		Func<bool> silentSessionProvider,
		bool useDelayedScheduling = false)
	{
		_contentProvider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));
		_silentSessionProvider = silentSessionProvider ?? throw new ArgumentNullException(nameof(silentSessionProvider));
		_contentChangedWorker = new ContentChangedWorker();

		if (useDelayedScheduling)
		{
			_textChangedDelayedTimer = new DispatcherTimer();
			_textChangedDelayedTimer.Tick += TextChangedDelayedTimer_Tick;
			_textChangedDelayedTimer.Interval = _delayedInterval;
		}

		_contentChangedWorker.RunWorkerCompleted += ContentChangedWorker_RunWorkerCompleted;
	}

	public event EventHandler? ContentChangedWorkerRunCompleted;

	public event EventHandler? TextChangedDelayed;

	public string FilePath
	{
		get => _contentChangedWorker.FilePath;
		set => _contentChangedWorker.FilePath = value;
	}

	public bool CreateBackupFiles
	{
		get => _contentChangedWorker.CreateBackupFiles;
		set => _contentChangedWorker.CreateBackupFiles = value;
	}

	public TimeSpan DelayedInterval
	{
		get => _textChangedDelayedTimer?.Interval ?? _delayedInterval;
		set
		{
			_delayedInterval = value;

			if (_textChangedDelayedTimer is not null)
				_textChangedDelayedTimer.Interval = value;
		}
	}

	public bool HandleContentChanged()
	{
		bool isChanged = _contentChangedWorker.HasChanges(_contentProvider());

		if (_textChangedDelayedTimer is not null)
		{
			_textChangedDelayedTimer.Stop();
			_textChangedDelayedTimer.Start();
		}

		return isChanged;
	}

	public bool RunContentChangedCheck()
	{
		string content = _contentProvider();
		bool isChanged = _contentChangedWorker.HasChanges(content);

		if (!_silentSessionProvider())
			_contentChangedWorker.RunAsync(content);

		return isChanged;
	}

	public void SetPersistedContent(string content)
		=> _contentChangedWorker.SetPersistedContent(content);

	public bool HasChanges(string content)
		=> _contentChangedWorker.HasChanges(content);

	public void Dispose()
	{
		if (_textChangedDelayedTimer is not null)
		{
			_textChangedDelayedTimer.Stop();
			_textChangedDelayedTimer.Tick -= TextChangedDelayedTimer_Tick;
		}

		_contentChangedWorker.RunWorkerCompleted -= ContentChangedWorker_RunWorkerCompleted;
		_contentChangedWorker.Dispose();
	}

	private void ContentChangedWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
		=> ContentChangedWorkerRunCompleted?.Invoke(this, EventArgs.Empty);

	private void TextChangedDelayedTimer_Tick(object? sender, EventArgs e)
	{
		RunContentChangedCheck();
		TextChangedDelayed?.Invoke(this, EventArgs.Empty);

		if (_textChangedDelayedTimer is not null)
			_textChangedDelayedTimer.Stop();
	}
}
