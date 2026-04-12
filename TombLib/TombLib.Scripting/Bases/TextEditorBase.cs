using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Rendering;
using TombLib.Scripting.Resources;
using TombLib.Scripting.Utils;
using TombLib.Scripting.Workers;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.Bases
{
	public abstract class TextEditorBase : TextEditor, IEditorControl, ISupportsFindReplace
	{
		protected const double ToolTipTextMaxWidth = 500.0;
		protected static readonly double ToolTipTextFontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0);
		protected static readonly SolidColorBrush DefaultToolTipBorder = TextEditorColorPalette.ToolTipBorder;
		protected static readonly SolidColorBrush DefaultToolTipBackground = TextEditorColorPalette.ToolTipBackground;
		private static readonly SolidColorBrush ErrorToolTipBorder = TextEditorColorPalette.ErrorToolTipBorder;
		private static readonly SolidColorBrush ErrorToolTipBackground = TextEditorColorPalette.ErrorToolTipBackground;
		private static readonly SolidColorBrush WarningToolTipBorder = TextEditorColorPalette.WarningToolTipBorder;
		private static readonly SolidColorBrush WarningToolTipBackground = TextEditorColorPalette.WarningToolTipBackground;
		private static readonly SolidColorBrush InformationToolTipBorder = TextEditorColorPalette.InformationToolTipBorder;
		private static readonly SolidColorBrush InformationToolTipBackground = TextEditorColorPalette.InformationToolTipBackground;
		private static readonly SolidColorBrush HintToolTipBorder = TextEditorColorPalette.HintToolTipBorder;
		private static readonly SolidColorBrush HintToolTipBackground = TextEditorColorPalette.HintToolTipBackground;
		protected static readonly SolidColorBrush ToolTipForeground = TextEditorColorPalette.ToolTipForeground;

		public EditorType EditorType => EditorType.Text;
		public abstract string DefaultFileExtension { get; }

		#region Properties

		public string FilePath
		{
			get => Document.FileName;
			set
			{
				Document.FileName = value;
				_contentChangedWorker.FilePath = value;
			}
		}

		public bool IsSilentSession { get; set; }

		public bool CreateBackupFiles
		{
			get => IsSilentSession ? false : _contentChangedWorker.CreateBackupFiles;
			set => _contentChangedWorker.CreateBackupFiles = value;
		}

		public string Content
		{
			get => Text;
			set => SetContent(value);
		}

		public bool IsContentChanged { get; set; }

		public DateTime LastModified { get; set; }

		public int CurrentRow => TextArea.Caret.Position.Line;
		public int CurrentColumn => TextArea.Caret.Position.Column;

		public object SelectedContent => SelectedText;

		public int MinZoom { get; set; } = 25;
		public int MaxZoom { get; set; } = 400;
		public int ZoomStepSize { get; set; } = 15;

		public string CommentPrefix { get; set; } = string.Empty;

		public TimeSpan TextChangedDelayedInterval
		{
			get => _textChangedDelayedTimer.Interval;
			set => _textChangedDelayedTimer.Interval = value;
		}

		public string ParenthesesClosingString { get; set; } = ")";
		public string BracesClosingString { get; set; } = "}";
		public string BracketsClosingString { get; set; } = "]";
		public string QuotesClosingString { get; set; } = "\"";

		public Version EngineVersion { get; set; } = new Version(0, 0);

		#endregion Properties

		#region Configuration

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

		#endregion Configuration

		#region Fields

		protected Popup _specialToolTip = new Popup();
		protected CompletionWindow _completionWindow;

		private ContentChangedWorker _contentChangedWorker;

		private DispatcherTimer _textChangedDelayedTimer = new DispatcherTimer();
		private DispatcherTimer _toolTipCloseTimer = new DispatcherTimer();
		private bool _toolTipContentHovered;
		private readonly Border _specialToolTipBorder = new Border();
		private readonly ContentPresenter _specialToolTipPresenter = new ContentPresenter();
		private readonly List<TextAnchor> _bookmarkAnchors = new List<TextAnchor>();
		private IReadOnlyList<TextEditorDiagnostic> _diagnostics = Array.Empty<TextEditorDiagnostic>();

		private IBackgroundRenderer _bookmarkRenderer;
		private IBackgroundRenderer _errorRenderer;

		internal IReadOnlyList<TextEditorDiagnostic> Diagnostics => _diagnostics;

		#endregion Fields

		#region Construction

		public TextEditorBase(Version engineVersion)
		{
			SetNewDefaultSettings();

			InitializeBackgroundWorkers();
			InitializeTimers();
			InitializeToolTip();
			InitializeRenderers();

			BindEventMethods();

			EngineVersion = engineVersion;
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

		private void InitializeBackgroundWorkers()
		{
			_contentChangedWorker = new ContentChangedWorker();
			_contentChangedWorker.RunWorkerCompleted += ContentChangedWorker_RunWorkerCompleted;
		}

		private void InitializeTimers()
		{
			TextChangedDelayedInterval = new TimeSpan(0, 0, 0, 0, 300);
			_textChangedDelayedTimer.Tick += TextChangedDelayedTimer_Tick;
			_toolTipCloseTimer.Interval = new TimeSpan(0, 0, 0, 0, 900);
			_toolTipCloseTimer.Tick += ToolTipCloseTimer_Tick;
		}

		private void InitializeToolTip()
		{
			_specialToolTip.AllowsTransparency = true;
			_specialToolTip.PopupAnimation = PopupAnimation.None;
			_specialToolTip.StaysOpen = true;
			_specialToolTip.Placement = PlacementMode.RelativePoint;

			_specialToolTipBorder.SnapsToDevicePixels = true;
			_specialToolTipBorder.CornerRadius = new CornerRadius(3.0);
			_specialToolTipBorder.BorderThickness = new Thickness(1.0);
			_specialToolTipBorder.Padding = new Thickness(8.0, 6.0, 8.0, 6.0);
			_specialToolTipBorder.Child = _specialToolTipPresenter;
			_specialToolTipBorder.MouseEnter += SpecialToolTip_MouseEnter;
			_specialToolTipBorder.MouseLeave += SpecialToolTip_MouseLeave;

			_specialToolTip.Child = _specialToolTipBorder;
		}

		private void InitializeRenderers()
		{
			_bookmarkRenderer = new BookmarkRenderer(this);
			_errorRenderer = new ErrorRenderer(this);

			TextArea.TextView.BackgroundRenderers.Add(_bookmarkRenderer);
			TextArea.TextView.BackgroundRenderers.Add(_errorRenderer);
		}

		private void BindEventMethods()
		{
			TextArea.Caret.PositionChanged += delegate { OnStatusChanged(EventArgs.Empty); };
			TextArea.SelectionChanged += delegate { OnStatusChanged(EventArgs.Empty); };

			TextArea.TextEntering += TextArea_TextEntering;
			TextChanged += TextEditor_TextChanged;

			MouseHover += TextEditor_MouseHover;
			MouseHoverStopped += TextEditor_MouseHoverStopped;

			PreviewMouseWheel += TextEditor_PreviewMouseWheel;
			MouseRightButtonDown += TextEditor_MouseRightButtonDown;
		}

		#endregion Construction

		#region Events

		public event EventHandler StatusChanged;
		protected virtual void OnStatusChanged(EventArgs e)
			=> StatusChanged?.Invoke(this, e);

		public event EventHandler ZoomChanged;
		protected virtual void OnZoomChanged(EventArgs e)
		{
			ZoomChanged?.Invoke(this, e);
			OnStatusChanged(EventArgs.Empty);
		}

		public event EventHandler TextChangedDelayed;
		protected virtual void OnTextChangedDelayed(EventArgs e)
			=> TextChangedDelayed?.Invoke(this, e);

		public event EventHandler ContentChangedWorkerRunCompleted;
		protected virtual void OnContentChangedWorkerRunCompleted(EventArgs e)
			=> ContentChangedWorkerRunCompleted?.Invoke(this, e);

		private void ContentChangedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			LastModified = DateTime.Now;
			IsContentChanged = (bool)e.Result;
			OnContentChangedWorkerRunCompleted(EventArgs.Empty);
		}

		private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			CloseDefinitionToolTip(true); // Prevents the ToolTip from covering the screen while typing
			HandleAutoClosing(e);
		}

		private void TextEditor_TextChanged(object sender, EventArgs e)
		{
			IsContentChanged = true;

			_textChangedDelayedTimer.Stop();
			_textChangedDelayedTimer.Start();
		}

		private void TextChangedDelayedTimer_Tick(object sender, EventArgs e)
		{
			TryRunContentChangedWorker();

			OnTextChangedDelayed(EventArgs.Empty);
			_textChangedDelayedTimer.Stop();
		}

		private void TextEditor_MouseHover(object sender, MouseEventArgs e)
			=> HandleMouseHover(e);

		protected virtual void HandleMouseHover(MouseEventArgs e)
			=> HandleErrorToolTips(e);

		private void TextEditor_MouseHoverStopped(object sender, MouseEventArgs e)
			=> ScheduleDefinitionToolTipClose();

		private void TextEditor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
				HandleZoom(e);
		}

		private void TextEditor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
			=> MoveCaretToMousePosition();

		private void ToolTipCloseTimer_Tick(object sender, EventArgs e)
		{
			_toolTipCloseTimer.Stop();

			if (!_toolTipContentHovered && !_specialToolTipBorder.IsMouseOver)
				CloseDefinitionToolTip(true);
		}

		private void SpecialToolTip_MouseEnter(object sender, MouseEventArgs e)
		{
			_toolTipContentHovered = true;
			_toolTipCloseTimer.Stop();
		}

		private void SpecialToolTip_MouseLeave(object sender, MouseEventArgs e)
		{
			_toolTipContentHovered = false;
			ScheduleDefinitionToolTipClose();
		}

		protected void CloseDefinitionToolTip(bool force = false)
		{
			_toolTipCloseTimer.Stop();

			if (!force && (_toolTipContentHovered || _specialToolTipBorder.IsMouseOver))
				return;

			if (_specialToolTip.IsOpen)
				_specialToolTip.IsOpen = false;

			_specialToolTipPresenter.Content = null;
			_toolTipContentHovered = false;
		}

		private void ScheduleDefinitionToolTipClose()
		{
			if (!_specialToolTip.IsOpen)
				return;

			_toolTipCloseTimer.Stop();
			_toolTipCloseTimer.Start();
		}

		private void MoveCaretToMousePosition()
		{
			if (string.IsNullOrEmpty(SelectedText))
			{
				TextViewPosition? position = GetTextViewPosition(Mouse.GetPosition(this));

				if (position != null)
				{
					SelectionStart = Document.GetOffset(new TextLocation(position.Value.Line, position.Value.Column));
					SelectionLength = 0;
				}
			}
		}

		#endregion Events

		#region File I/O

		public new void Load(string filePath)
			=> Load(filePath, false);

		public void Load(string filePath, bool silentSession)
		{
			base.Load(filePath);
			FilePath = filePath;

			IsContentChanged = false;
			IsSilentSession = silentSession;

			RestoreBookmarks();
		}

		public void Save()
			=> Save(FilePath);

		public new void Save(string filePath)
		{
			base.Save(filePath);

			TryRunContentChangedWorker();
		}

		private void SaveBookmarks()
		{
			if (string.IsNullOrWhiteSpace(FilePath))
				return;

			List<DocumentLine> bookmarkedLines = CollectBookmarkedLines();

			var builder = new StringBuilder();

			foreach (DocumentLine line in bookmarkedLines)
				builder.AppendLine(line.LineNumber.ToString());

			try
			{
				string bookmarkFileName = FilePath + ".bkmrk";

				if (bookmarkedLines.Count > 0)
					File.WriteAllText(bookmarkFileName, builder.ToString());
				else if (File.Exists(bookmarkFileName))
					File.Delete(bookmarkFileName);
			}
			catch
			{
				// Too bad.
			}
		}

		private void RestoreBookmarks()
		{
			_bookmarkAnchors.Clear();

			string bookmarkFileName = FilePath + ".bkmrk";

			if (!File.Exists(bookmarkFileName))
				return;

			try
			{
				foreach (string line in File.ReadAllLines(bookmarkFileName))
				{
					if (int.TryParse(line, out int lineNumber) && lineNumber >= 1 && lineNumber <= Document.LineCount)
					{
						DocumentLine documentLine = Document.GetLineByNumber(lineNumber);

						if (FindBookmarkAnchor(documentLine) is null)
							AddBookmark(documentLine);
					}
				}
			}
			catch
			{
				// Too bad.
			}
		}

		#endregion File I/O

		#region Content

		public void TryRunContentChangedWorker()
		{
			if (!IsSilentSession && !_contentChangedWorker.IsBusy)
				_contentChangedWorker.RunAsync(Content);
		}

		private void SetContent(string content)
		{
			DocumentLine cachedLine = Document.GetLineByOffset(CaretOffset);

			Document.UndoStack.StartUndoGroup();

			SelectAll();
			SelectedText = content;

			Document.UndoStack.EndUndoGroup();

			if (cachedLine.EndOffset <= Document.TextLength)
				ResetSelectionAt(cachedLine);
			else
				ResetSelection();

			TryRunContentChangedWorker();
		}

		#endregion Content

		#region Error handling

		public void SetDiagnostics(IReadOnlyList<TextEditorDiagnostic> diagnostics)
		{
			_diagnostics = diagnostics ?? Array.Empty<TextEditorDiagnostic>();
			InvalidateDiagnosticLayer();
		}

		public void ClearDiagnostics()
		{
			if (_diagnostics.Count == 0)
				return;

			_diagnostics = Array.Empty<TextEditorDiagnostic>();
			InvalidateDiagnosticLayer();
		}

		private void InvalidateDiagnosticLayer()
		{
			TextArea.TextView.InvalidateLayer(KnownLayer.Background);
			TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
			TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
			TextArea.TextView.InvalidateVisual();
		}

		private void HandleErrorToolTips(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			TryShowDiagnosticToolTip(hoveredOffset);
		}

		protected bool TryGetDiagnosticInfo(int hoveredOffset, out string message, out TextEditorDiagnosticSeverity severity)
		{
			message = null;
			severity = TextEditorDiagnosticSeverity.Error;

			if (!LiveErrorUnderlining || _diagnostics.Count == 0)
				return false;

			List<TextEditorDiagnostic> hoveredDiagnostics = GetDiagnosticsAtOffset(hoveredOffset);

			if (hoveredDiagnostics.Count == 0)
				hoveredDiagnostics = GetDiagnosticsForLine(Document.GetLineByOffset(hoveredOffset));

			if (hoveredDiagnostics.Count == 0)
				return false;

			severity = hoveredDiagnostics
				.OrderBy(diagnostic => diagnostic.Severity)
				.Select(diagnostic => diagnostic.Severity)
				.First();

			message = string.Join(Environment.NewLine + Environment.NewLine,
				hoveredDiagnostics
					.OrderBy(diagnostic => diagnostic.Severity)
					.ThenBy(diagnostic => diagnostic.StartOffset)
					.Select(FormatDiagnosticMessage)
					.Distinct(StringComparer.Ordinal));

			return true;
		}

		protected void ShowDiagnosticToolTip(string message, TextEditorDiagnosticSeverity severity)
		{
			GetDiagnosticToolTipColors(severity, out SolidColorBrush border, out SolidColorBrush background);
			ShowToolTip(message, border, background, ToolTipForeground);
		}

		protected bool TryShowDiagnosticToolTip(int hoveredOffset)
		{
			if (!TryGetDiagnosticInfo(hoveredOffset, out string message, out TextEditorDiagnosticSeverity severity))
				return false;

			ShowDiagnosticToolTip(message, severity);
			return true;
		}

		protected bool HasDiagnosticsOnLine(DocumentLine line)
			=> line is not null && GetDiagnosticsForLine(line).Count > 0;

		#endregion Error handling

		#region Auto bracket closing

		private void HandleAutoClosing(TextCompositionEventArgs e)
		{
			if (AutoCloseParentheses)
			{
				if (e.Text == "(")
					PerformAutoClosing(ParenthesesClosingString);
				else if (e.Text == ParenthesesClosingString)
					TryPerformElementSkip(e, ParenthesesClosingString);
			}

			if (AutoCloseBraces)
			{
				if (e.Text == "{")
					PerformAutoClosing(BracesClosingString);
				else if (e.Text == BracesClosingString)
					TryPerformElementSkip(e, BracesClosingString);
			}

			if (AutoCloseBrackets)
			{
				if (e.Text == "[")
					PerformAutoClosing(BracketsClosingString);
				else if (e.Text == BracketsClosingString)
					TryPerformElementSkip(e, BracketsClosingString);
			}

			if (AutoCloseQuotes)
			{
				if (e.Text == "\"" && !(CaretOffset < Document.TextLength && Document.GetCharAt(CaretOffset) == QuotesClosingString[0]))
					PerformAutoClosing(QuotesClosingString);
				else if (e.Text == QuotesClosingString)
					TryPerformElementSkip(e, QuotesClosingString);
			}
		}

		private void PerformAutoClosing(string closingElement)
		{
			SelectedText += closingElement;
			CaretOffset -= closingElement.Length;

			SelectionStart = CaretOffset;
			SelectionLength = 0;
		}

		private void TryPerformElementSkip(TextCompositionEventArgs e, string element)
		{
			if (CaretOffset < Document.TextLength && Document.GetCharAt(CaretOffset) == element[0])
			{
				CaretOffset++;
				e.Handled = true;
			}
		}

		#endregion Auto bracket closing

		#region Multiline commenting

		// TODO: Refactor

		public void CommentOutLines()
		{
			DocumentLine startLine = Document.GetLineByOffset(SelectionStart);
			DocumentLine endLine = Document.GetLineByOffset(SelectionStart + SelectionLength);

			int totalLineLength = 0;

			var builder = new StringBuilder();

			for (int i = startLine.LineNumber; i <= endLine.LineNumber; i++)
			{
				DocumentLine currentLine = Document.GetLineByNumber(i);
				string currentLineText = Document.GetText(currentLine.Offset, currentLine.Length);

				var whitespaceBuilder = new StringBuilder();

				for (int j = 0; j < currentLineText.Length; j++)
				{
					char c = currentLineText[j];

					if (char.IsWhiteSpace(c))
						whitespaceBuilder.Append(c);
					else
						break;
				}

				if (!string.IsNullOrWhiteSpace(currentLineText))
					builder.AppendLine(whitespaceBuilder.ToString() + CommentPrefix + currentLineText.TrimStart());

				totalLineLength += currentLine.TotalLength;
			}

			Select(startLine.Offset, totalLineLength);
			SelectedText = builder.ToString();

			Select(startLine.Offset, SelectionLength - 1);
		}

		public void UncommentLines()
		{
			DocumentLine startLine = Document.GetLineByOffset(SelectionStart);
			DocumentLine endLine = Document.GetLineByOffset(SelectionStart + SelectionLength);

			int totalLineLength = 0;

			var builder = new StringBuilder();

			for (int i = startLine.LineNumber; i <= endLine.LineNumber; i++)
			{
				DocumentLine currentLine = Document.GetLineByNumber(i);
				string currentLineText = Document.GetText(currentLine.Offset, currentLine.Length);

				var whitespaceBuilder = new StringBuilder();

				for (int j = 0; j < currentLineText.Length; j++)
				{
					char c = currentLineText[j];

					if (char.IsWhiteSpace(c))
						whitespaceBuilder.Append(c);
					else
						break;
				}

				if (currentLineText.TrimStart().StartsWith(CommentPrefix))
					builder.AppendLine(whitespaceBuilder.ToString() + currentLineText.TrimStart().Remove(0, CommentPrefix.Length));
				else
					builder.AppendLine(currentLineText);

				totalLineLength += currentLine.TotalLength;
			}

			Select(startLine.Offset, totalLineLength);
			SelectedText = builder.ToString();

			Select(startLine.Offset, SelectionLength - 1);
		}

		#endregion Multiline commenting

		#region Bookmarks

		// TODO: Refactor

		public void ToggleBookmark()
		{
			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);

			TextAnchor bookmarkAnchor = FindBookmarkAnchor(currentLine);

			if (bookmarkAnchor is null)
				AddBookmark(currentLine);
			else
				_bookmarkAnchors.Remove(bookmarkAnchor);

			TextArea.TextView.InvalidateLayer(KnownLayer.Background);

			SaveBookmarks();
		}

		public void GoToNextBookmark()
		{
			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);
			List<DocumentLine> bookmarkedLines = CollectBookmarkedLines();

			if (bookmarkedLines.Count == 0)
				return;

			DocumentLine nextBookmark = bookmarkedLines.FirstOrDefault(line => line.LineNumber > currentLine.LineNumber)
				?? bookmarkedLines[0];

			CaretOffset = nextBookmark.EndOffset;
			ScrollToLine(nextBookmark.LineNumber);
		}

		public void GoToPrevBookmark()
		{
			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);
			List<DocumentLine> bookmarkedLines = CollectBookmarkedLines();

			if (bookmarkedLines.Count == 0)
				return;

			DocumentLine previousBookmark = bookmarkedLines.LastOrDefault(line => line.LineNumber < currentLine.LineNumber)
				?? bookmarkedLines[bookmarkedLines.Count - 1];

			CaretOffset = previousBookmark.EndOffset;
			ScrollToLine(previousBookmark.LineNumber);
		}

		#endregion Bookmarks

		#region Zoom

		private int _zoom = 100;

		public int Zoom
		{
			get => _zoom;
			set
			{
				FontSize = DefaultFontSize * value / 100;
				_zoom = value;
			}
		}

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

		#region CompletionWindow

		public void InitializeCompletionWindow(int width = 300, int height = 300)
		{
			_completionWindow = new CompletionWindow(TextArea)
			{
				WindowStyle = WindowStyle.None,
				ResizeMode = ResizeMode.NoResize,
				BorderThickness = new Thickness(1.0),
				Background = DefaultToolTipBackground,
				Foreground = ToolTipForeground,
				BorderBrush = DefaultToolTipBorder,
				Width = width,
				Height = height
			};
		}

		public void ShowCompletionWindow()
		{
			_completionWindow.Show();
			_completionWindow.Closed += delegate { _completionWindow = null; };
		}

		#endregion CompletionWindow

		#region Other public methods

		public void ClearAllBookmarks(System.Windows.Forms.IWin32Window promptOwner)
		{
			System.Windows.Forms.DialogResult result = DarkMessageBox.Show(promptOwner, "Are you sure you want to clear all bookmarks from the current document?",
					"Are you sure?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);

			if (result == System.Windows.Forms.DialogResult.Yes)
				_bookmarkAnchors.Clear();

			TextArea.TextView.InvalidateLayer(KnownLayer.Background);

			SaveBookmarks();
		}

		public void ConvertSpacesToTabs()
			=> Content = WhiteSpaceConverter.ConvertSpacesToTabs(Content, 4);

		public void ConvertTabsToSpaces()
			=> Content = WhiteSpaceConverter.ConvertTabsToSpaces(Content, 4);

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
			TextViewPosition? position = GetTextViewPosition(point);

			if (position == null)
				return -1;

			DocumentLine pointLine = Document.GetLineByNumber(((TextViewPosition)position).Line);
			int offset = pointLine.Offset + Math.Min(pointLine.Length, Math.Max(0, ((TextViewPosition)position).Column - 1));

			if (offset > Document.TextLength)
				return -1;
			else
				return offset;
		}

		private TextViewPosition? GetTextViewPosition(Point point)
		{
			if (TextArea?.TextView is null)
				return null;

			Point textViewPoint = TranslatePoint(point, TextArea.TextView);
			return TextArea.TextView.GetPosition(textViewPoint + TextArea.TextView.ScrollOffset);
		}

		public string GetWordFromOffset(int offset)
		{
			int wordStart = TextUtilities.GetNextCaretPosition(Document, offset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);
			int wordEnd = TextUtilities.GetNextCaretPosition(Document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);

			if (wordStart >= 0 && wordEnd >= 0)
				return Document.GetText(wordStart, wordEnd - wordStart);

			return null;
		}

		public void ShowToolTip(string content)
			=> ShowToolTip(content,
				DefaultToolTipBorder,
				DefaultToolTipBackground,
				ToolTipForeground);

		public void ShowMarkdownToolTip(string content)
			=> ShowMarkdownToolTip(content,
				DefaultToolTipBorder,
				DefaultToolTipBackground,
				ToolTipForeground);

		public void ShowToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
			=> ShowToolTip(CreatePlainToolTipContent(content, foreground), border, background);

		public void ShowMarkdownToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
			=> ShowToolTip(CreateMarkdownToolTipContent(content, foreground, background), border, background);

		protected void ShowToolTip(object content, SolidColorBrush border, SolidColorBrush background)
		{
			_toolTipCloseTimer.Stop();
			_toolTipContentHovered = false;
			_specialToolTip.PlacementTarget = this;
			Point mousePosition = Mouse.GetPosition(this);
			_specialToolTip.HorizontalOffset = mousePosition.X + 14.0;
			_specialToolTip.VerticalOffset = mousePosition.Y + 20.0;

			_specialToolTipBorder.BorderBrush = border;
			_specialToolTipBorder.Background = background;
			_specialToolTipPresenter.Content = content;
			_specialToolTip.IsOpen = true;
		}

		private static bool IsSeverityPrefixed(string message)
			=> !string.IsNullOrWhiteSpace(message)
				&& (message.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)
					|| message.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase)
					|| message.StartsWith("Information:", StringComparison.OrdinalIgnoreCase)
					|| message.StartsWith("Hint:", StringComparison.OrdinalIgnoreCase)
					|| message.StartsWith("Diagnostic:", StringComparison.OrdinalIgnoreCase));

		private static object CreatePlainToolTipContent(string content, Brush foreground)
			=> MarkdownToolTipRenderer.CreatePlainTextContent(content, foreground);

		private static object CreateMarkdownToolTipContent(string content, Brush foreground, Brush background)
		{
			string normalizedContent = NormalizeToolTipLineEndings(content);

			if (string.IsNullOrWhiteSpace(normalizedContent))
				return CreatePlainToolTipContent(string.Empty, foreground);

			return MarkdownToolTipRenderer.CreateContent(normalizedContent, foreground, background);
		}

		private static string NormalizeToolTipLineEndings(string text)
			=> (text ?? string.Empty)
				.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Replace('\r', '\n');

		private List<DocumentLine> CollectBookmarkedLines()
		{
			var bookmarkedLines = new List<DocumentLine>();
			var invalidAnchors = new List<TextAnchor>();
			var seenLineNumbers = new HashSet<int>();

			foreach (TextAnchor anchor in _bookmarkAnchors)
			{
				DocumentLine line = GetBookmarkedLine(anchor);

				if (line is null)
				{
					invalidAnchors.Add(anchor);
					continue;
				}

				if (seenLineNumbers.Add(line.LineNumber))
					bookmarkedLines.Add(line);
			}

			foreach (TextAnchor anchor in invalidAnchors)
				_bookmarkAnchors.Remove(anchor);

			bookmarkedLines.Sort((left, right) => left.LineNumber.CompareTo(right.LineNumber));
			return bookmarkedLines;
		}

		internal IReadOnlyList<DocumentLine> GetBookmarkedLines()
			=> CollectBookmarkedLines();

		private void AddBookmark(DocumentLine line)
		{
			if (line is null)
				return;

			var anchor = Document.CreateAnchor(line.Offset);
			anchor.MovementType = AnchorMovementType.BeforeInsertion;
			anchor.SurviveDeletion = true;
			_bookmarkAnchors.Add(anchor);
		}

		private TextAnchor FindBookmarkAnchor(DocumentLine line)
		{
			if (line is null)
				return null;

			foreach (TextAnchor anchor in _bookmarkAnchors)
			{
				DocumentLine bookmarkedLine = GetBookmarkedLine(anchor);

				if (bookmarkedLine is not null && bookmarkedLine.LineNumber == line.LineNumber)
					return anchor;
			}

			return null;
		}

		private DocumentLine GetBookmarkedLine(TextAnchor anchor)
		{
			if (anchor is null || anchor.IsDeleted || Document.LineCount == 0)
				return null;

			int offset = Math.Max(0, Math.Min(anchor.Offset, Document.TextLength));
			return Document.GetLineByOffset(offset);
		}

		private List<TextEditorDiagnostic> GetDiagnosticsAtOffset(int offset)
			=> _diagnostics
				.Where(diagnostic => diagnostic.ContainsOffset(offset))
				.ToList();

		private List<TextEditorDiagnostic> GetDiagnosticsForLine(DocumentLine line)
		{
			if (line is null || _diagnostics.Count == 0)
				return new List<TextEditorDiagnostic>();

			int endOffset = Math.Max(line.EndOffset, line.Offset + 1);

			return _diagnostics
				.Where(diagnostic => diagnostic.Intersects(line.Offset, endOffset))
				.ToList();
		}

		private static string FormatDiagnosticMessage(TextEditorDiagnostic diagnostic)
		{
			if (diagnostic is null)
				return string.Empty;

			if (IsSeverityPrefixed(diagnostic.Message))
				return diagnostic.Message;

			return diagnostic.Severity.GetLabel() + ":\n" + diagnostic.Message;
		}

		protected static void GetDiagnosticToolTipColors(TextEditorDiagnosticSeverity severity,
			out SolidColorBrush border, out SolidColorBrush background)
		{
			switch (severity)
			{
				case TextEditorDiagnosticSeverity.Warning:
					border = WarningToolTipBorder;
					background = WarningToolTipBackground;
					break;

				case TextEditorDiagnosticSeverity.Information:
					border = InformationToolTipBorder;
					background = InformationToolTipBackground;
					break;

				case TextEditorDiagnosticSeverity.Hint:
					border = HintToolTipBorder;
					background = HintToolTipBackground;
					break;

				default:
					border = ErrorToolTipBorder;
					background = ErrorToolTipBackground;
					break;
			}
		}

		public virtual void UpdateSettings(ConfigurationBase configuration)
		{
			var config = configuration as TextEditorConfigBase;

			FontSize = config.FontSize;
			DefaultFontSize = config.FontSize;
			FontFamily = new FontFamily(config.FontFamily);

			Document.UndoStack.SizeLimit = config.UndoStackSize;

			AutocompleteEnabled = config.AutocompleteEnabled;
			LiveErrorUnderlining = config.LiveErrorUnderlining;

			AutoCloseParentheses = config.AutoCloseParentheses;
			AutoCloseBraces = config.AutoCloseBraces;
			AutoCloseBrackets = config.AutoCloseBrackets;
			AutoCloseQuotes = config.AutoCloseQuotes;

			WordWrap = config.WordWrapping;

			Options.HighlightCurrentLine = config.HighlightCurrentLine;

			ShowLineNumbers = config.ShowLineNumbers;

			Options.ShowSpaces = config.ShowVisualSpaces;
			Options.ShowTabs = config.ShowVisualTabs;
		}

		public virtual void TidyCode(bool trimOnly = false)
			=> Content = BasicCleaner.TrimEndingWhitespace(Content);

		#endregion Other public methods

		#region IEditorControl methods

		void IEditorControl.Undo() => Undo();
		void IEditorControl.Redo() => Redo();

		public virtual void GoToObject(string objectName, object identifyingObject = null)
		{ } // Bruh

		public void Dispose()
			=> _contentChangedWorker?.Dispose();

		#endregion IEditorControl methods
	}
}
