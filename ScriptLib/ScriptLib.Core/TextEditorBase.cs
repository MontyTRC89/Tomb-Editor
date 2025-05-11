using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ScriptLib.Core.Enums;
using ScriptLib.Core.Interfaces;
using ScriptLib.Core.Renderers;
using ScriptLib.Core.Structs;
using ScriptLib.Core.Workers;
using ScriptLib.Core.Workers.Implementations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScriptLib.Core;

public abstract class TextEditorBase : TextEditor, IEditorControl, ISupportsBookmarks, ISupportsErrorChecking, ISupportsGoToObject
{
	public EditorType EditorType => EditorType.Text;
	public abstract string DefaultFileExtension { get; }
	public abstract string CommentPrefix { get; }

	#region Properties

	/* Editor state properties */

	public string? FilePath => Document.FileName;

	public bool IsContentChanged { get; private set; }
	public DateTime LastModified { get; private set; }

	public Version GameEngineVersion { get; }
	public bool IsSilentSession { get; }

	public TimeSpan ContentChangedDelayedInterval
	{
		get => _contentChangedDelayedTimer.Interval;
		set => _contentChangedDelayedTimer.Interval = value;
	}

	public SortedSet<int> Bookmarks { get; set; } = [];
	public IReadOnlyList<DocumentError> Errors { get; set; } = [];

	/* Editing state properties */

	public int CurrentRow => TextArea.Caret.Position.Line;
	public int CurrentColumn => TextArea.Caret.Position.Column;

	public object SelectedContent => SelectedText;

	/* Zoom properties */

	public int Zoom
	{
		get => _zoom;
		set
		{
			FontSize = DefaultFontSize * value / 100;
			_zoom = value;
		}
	}

	public int MinZoom { get; set; } = 25;
	public int MaxZoom { get; set; } = 400;
	public int ZoomStepSize { get; set; } = 15;

	/* Closing strings for auto-closing brackets */

	public virtual string ParenthesesClosingString { get; } = ")";
	public virtual string BracesClosingString { get; } = "}";
	public virtual string BracketsClosingString { get; } = "]";
	public virtual string QuotesClosingString { get; } = "\"";

	/* Configuration properties */

	/// <summary>
	/// Basically FontSize but zooming doesn't affect its value.
	/// </summary>
	public double DefaultFontSize { get; set; } = TextEditorBaseDefaults.FontSize;

	public bool AutocompleteEnabled { get; set; } = TextEditorBaseDefaults.AutocompleteEnabled;
	public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;

	public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;
	public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;
	public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;
	public bool AutoCloseQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseQuotes;

	#endregion Properties

	#region Fields

	private readonly DispatcherTimer _contentChangedDelayedTimer;
	private readonly IFileContentWorker? _fileContentWorker;

	private readonly IBackgroundRenderer _bookmarkRenderer;
	private readonly IBackgroundRenderer _errorRenderer;

	private int _zoom = 100;

	private bool _lastAutoCloseApplied;
	private int _lastAutoClosePosition;
	private string? _lastAutoCloseChar;

	private bool _disposed;

	#endregion Fields

	#region Construction

	protected TextEditorBase(Version gameEngineVersion, string? filePath, bool isSilentSession = false)
	{
		Document.FileName = filePath;

		GameEngineVersion = gameEngineVersion;
		IsSilentSession = isSilentSession;

		// Initialize the file content worker
		if (!string.IsNullOrWhiteSpace(filePath) && !isSilentSession)
			_fileContentWorker = new FileContentWorker(filePath, true);

		_contentChangedDelayedTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(500)
		};

		_contentChangedDelayedTimer.Tick += ContentChangedDelayedTimer_Tick;

		// Initialize background renderers
		_bookmarkRenderer = new BookmarkRenderer(this);
		_errorRenderer = new ErrorRenderer(this);

		TextArea.TextView.BackgroundRenderers.Add(_bookmarkRenderer);
		TextArea.TextView.BackgroundRenderers.Add(_errorRenderer);

		// Bind event methods
		TextArea.Caret.PositionChanged += delegate { OnStatusChanged(EventArgs.Empty); };
		TextArea.SelectionChanged += delegate { OnStatusChanged(EventArgs.Empty); };

		TextArea.TextEntering += TextArea_TextEntering;
		TextChanged += TextEditor_TextChanged;

		PreviewMouseWheel += TextEditor_PreviewMouseWheel;
		MouseRightButtonDown += TextEditor_MouseRightButtonDown;

		PreviewKeyDown += TextEditor_PreviewKeyDown;

		// Set default settings
		SetNewDefaultSettings();
	}

	private void SetNewDefaultSettings()
	{
		Options.AllowScrollBelowDocument = true;
		TextArea.Margin = new Thickness(3, 0, 0, 0);

		FontWeight = FontWeights.Normal;

		TextArea.TextView.CurrentLineBackground = new SolidColorBrush(Color.FromArgb(16, 160, 160, 160));
		TextArea.TextView.CurrentLineBorder = new Pen(new SolidColorBrush(Color.FromArgb(24, 192, 192, 192)), 1);

		TextArea.SelectionCornerRadius = 0; // Why does this even exist?
		TextArea.SelectionBorder = new Pen(Brushes.SteelBlue, 1);

		HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
		VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
	}

	#endregion Construction

	#region Public methods

	public abstract void GoToObject(string objectNamePattern, bool isCaseSensitive, bool isExactMatch, bool isRegex);

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_contentChangedDelayedTimer.Stop();
				_fileContentWorker?.Dispose();
			}

			_disposed = true;
		}
	}

	public void GoToBookmark(int lineNumber)
	{
		DocumentLine? line = TryGetLineByNumber(lineNumber);

		if (line is null)
			return;

		CaretOffset = line.EndOffset;
		ScrollToLine(line.LineNumber);
	}

	public DocumentLine? TryGetLineByNumber(int lineNumber)
		=> lineNumber < 1 || lineNumber > Document.LineCount ? null : Document.GetLineByNumber(lineNumber);

	public DocumentLine? TryGetLineByOffset(int offset)
		=> offset < 0 || offset > Document.TextLength ? null : Document.GetLineByOffset(offset);

	public void SelectLine(int lineNumber) => SelectLine(Document.GetLineByNumber(lineNumber));
	public void SelectLine(DocumentLine line) => Select(line.Offset, line.Length);

	public void ReplaceLine(int lineNumber, string replacement, bool deselectAfterwards = false)
		=> ReplaceLine(Document.GetLineByNumber(lineNumber), replacement, deselectAfterwards);

	public void ReplaceLine(DocumentLine line, string replacement, bool deselectAfterwards = false)
	{
		SelectLine(line);
		SelectedText = replacement;

		if (deselectAfterwards)
			ResetSelection();
	}

	public void ReplaceContent(string newContent)
	{
		SelectAll();
		SelectedText = newContent;
		ResetSelection();
	}

	public void ResetSelection() => Select(Document.TextLength - 1, 0);

	public void ResetSelectionAt(int lineNumber) => ResetSelectionAt(Document.GetLineByNumber(lineNumber));
	public void ResetSelectionAt(DocumentLine line) => Select(line.EndOffset, 0);

	public int GetOffsetFromPoint(Point point)
	{
		TextViewPosition? position = GetPositionFromPoint(point);

		if (position is null)
			return -1;

		DocumentLine pointLine = Document.GetLineByNumber(((TextViewPosition)position).Line);
		int offset = pointLine.Offset + ((TextViewPosition)position).Column;

		return offset > Document.TextLength ? -1 : offset;
	}

	public virtual string? GetWordFromOffset(int offset)
	{
		int wordStart = TextUtilities.GetNextCaretPosition(Document, offset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);
		int wordEnd = TextUtilities.GetNextCaretPosition(Document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);

		return wordStart >= 0 && wordEnd >= 0
			? Document.GetText(wordStart, wordEnd - wordStart)
			: null;
	}

	#endregion Public methods

	#region Events

	public event EventHandler? ContentChangedDelayed;
	protected virtual void OnContentChangedDelayed(EventArgs e)
		=> ContentChangedDelayed?.Invoke(this, e);

	public event EventHandler? StatusChanged;
	protected virtual void OnStatusChanged(EventArgs e)
		=> StatusChanged?.Invoke(this, e);

	public event EventHandler? ZoomChanged;
	protected virtual void OnZoomChanged(EventArgs e)
	{
		ZoomChanged?.Invoke(this, e);
		OnStatusChanged(EventArgs.Empty);
	}

	#endregion Events

	#region Event handlers

	private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		=> HandleAutoClosing(e);

	private void TextEditor_TextChanged(object? sender, EventArgs e)
	{
		if (_fileContentWorker is null)
		{
			IsContentChanged = !string.IsNullOrWhiteSpace(FilePath) || Document.Text != string.Empty;
			LastModified = DateTime.Now;
		}

		if (_contentChangedDelayedTimer.IsEnabled)
			_contentChangedDelayedTimer.Stop();

		_contentChangedDelayedTimer.Start();
	}

	private async void ContentChangedDelayedTimer_Tick(object? sender, EventArgs e)
	{
		_contentChangedDelayedTimer.Stop();

		if (_fileContentWorker is not null)
		{
			IsContentChanged = await _fileContentWorker.HasContentChanged(Document.Text, Encoding);
			LastModified = IsContentChanged ? DateTime.Now : DateTime.MinValue;
		}

		OnContentChangedDelayed(EventArgs.Empty);
	}

	private void TextEditor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (Keyboard.Modifiers == ModifierKeys.Control)
			HandleZoom(e);
	}

	private void TextEditor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		=> MoveCaretToMousePosition();

	private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		// Handle backspace to remove both opening and closing characters when appropriate
		if (e.Key == Key.Back && _lastAutoCloseApplied && CaretOffset == _lastAutoClosePosition)
		{
			// Only do this if we're right after an auto-inserted opening character
			if (CaretOffset < Document.TextLength &&
				_lastAutoCloseChar != null &&
				CaretOffset + _lastAutoCloseChar.Length <= Document.TextLength &&
				Document.GetText(CaretOffset, _lastAutoCloseChar.Length) == _lastAutoCloseChar &&
				CaretOffset > 0)
			{
				Document.Remove(CaretOffset, _lastAutoCloseChar.Length); // Remove the entire closing string
																		 // The backspace will naturally remove the opening character
				_lastAutoCloseApplied = false;
			}
		}
		else if (e.Key != Key.Back)
		{
			// Reset tracking if any other key is pressed
			_lastAutoCloseApplied = false;
		}
	}

	#endregion Event handlers

	#region Auto bracket closing

	private void HandleAutoClosing(TextCompositionEventArgs e)
	{
		// Reset auto-close tracking since text is being entered
		_lastAutoCloseApplied = false;

		// Dictionary defining opening characters and their corresponding closing characters
		var autoCloseMapping = new Dictionary<string, (string, bool)>
		{
			{ "(", (ParenthesesClosingString, AutoCloseParentheses) },
			{ "{", (BracesClosingString, AutoCloseBraces) },
			{ "[", (BracketsClosingString, AutoCloseBrackets) },
			{ "\"", (QuotesClosingString, AutoCloseQuotes) }
		};

		// Handle closing characters - try to skip instead of inserting
		foreach (KeyValuePair<string, (string, bool)> pair in autoCloseMapping)
		{
			string closingChar = pair.Value.Item1;
			bool enabled = pair.Value.Item2;

			if (enabled && e.Text == closingChar && CaretOffset < Document.TextLength &&
				Document.GetCharAt(CaretOffset) == closingChar[0])
			{
				CaretOffset++;
				e.Handled = true;
				return;
			}
		}

		// Handle opening characters - insert closing character automatically
		if (autoCloseMapping.TryGetValue(e.Text, out (string, bool) closingInfo))
		{
			string closingChar = closingInfo.Item1;
			bool enabled = closingInfo.Item2;

			// Special case for quotes to avoid double quotes in existing text
			if (e.Text == "\"" && enabled)
			{
				if (!(CaretOffset < Document.TextLength && Document.GetCharAt(CaretOffset) == QuotesClosingString[0]))
					PerformAutoClosing(closingChar);
			}
			else if (enabled)
			{
				PerformAutoClosing(closingChar);
			}
		}
	}

	private void PerformAutoClosing(string closingElement)
	{
		SelectedText += closingElement;
		CaretOffset -= closingElement.Length;

		SelectionStart = CaretOffset;
		SelectionLength = 0;

		// Save information about this auto-close operation
		_lastAutoCloseApplied = true;
		_lastAutoClosePosition = CaretOffset + 1;
		_lastAutoCloseChar = closingElement;
	}

	#endregion Auto bracket closing

	#region Zoom

	private void HandleZoom(MouseWheelEventArgs e)
	{
		if (e.Delta > 0) // Increase
		{
			if (_zoom < MaxZoom)
			{
				_zoom += ZoomStepSize;
				FontSize = DefaultFontSize * _zoom / 100;
			}
		}
		else // Decrease
		{
			if (_zoom > MinZoom)
			{
				_zoom -= ZoomStepSize;
				FontSize = DefaultFontSize * _zoom / 100;
			}
		}

		// Invoke the ZoomChanged event
		OnZoomChanged(EventArgs.Empty);
		e.Handled = true;
	}

	#endregion Zoom

	#region Other private methods

	private void MoveCaretToMousePosition()
	{
		if (SelectionLength > 0)
			return;

		TextViewPosition? position = TextArea.TextView.GetPosition(Mouse.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);

		if (position is null)
			return;

		SelectionStart = Document.GetOffset(new TextLocation(position.Value.Line, position.Value.Column));
		SelectionLength = 0;
	}

	#endregion Other private methods
}
