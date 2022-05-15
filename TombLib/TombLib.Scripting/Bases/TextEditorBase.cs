using DarkUI.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Rendering;
using TombLib.Scripting.Resources;
using TombLib.Scripting.Utils;
using TombLib.Scripting.Workers;

namespace TombLib.Scripting.Bases
{
	public class TextEditorBase : TextEditor, IEditorControl, ISupportsFindReplace
	{
		public EditorType EditorType => EditorType.Text;

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

		protected ToolTip _specialToolTip = new ToolTip();
		protected CompletionWindow _completionWindow;

		private ContentChangedWorker _contentChangedWorker;

		private DispatcherTimer _textChangedDelayedTimer = new DispatcherTimer();

		private IBackgroundRenderer _bookmarkRenderer;
		private IBackgroundRenderer _errorRenderer;

		#endregion Fields

		#region Construction

		public TextEditorBase()
		{
			SetNewDefaultSettings();

			InitializeBackgroundWorkers();
			InitializeTimers();
			InitializeRenderers();

			BindEventMethods();
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
			=> _textChangedDelayedTimer.Tick += TextChangedDelayedTimer_Tick;

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
			CloseDefinitionToolTip(); // Prevents the ToolTip from covering the screen while typing
			HandleAutoClosing(e);
		}

		private void TextEditor_TextChanged(object sender, EventArgs e)
		{
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
			=> HandleErrorToolTips(e);

		private void TextEditor_MouseHoverStopped(object sender, MouseEventArgs e)
			=> CloseDefinitionToolTip();

		private void TextEditor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
				HandleZoom(e);
		}

		private void TextEditor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
			=> MoveCaretToMousePosition();

		private void CloseDefinitionToolTip()
		{
			if (_specialToolTip.IsOpen)
				_specialToolTip.IsOpen = false;
		}

		private void MoveCaretToMousePosition()
		{
			if (string.IsNullOrEmpty(SelectedText))
			{
				TextViewPosition? position = TextArea.TextView.GetPosition(Mouse.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);

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
			IEnumerable<DocumentLine> bookmarkedLines = Document.Lines.Where(line => line.IsBookmarked);

			var builder = new StringBuilder();

			foreach (DocumentLine line in bookmarkedLines)
				builder.AppendLine(line.LineNumber.ToString());

			try
			{
				string bookmarkFileName = FilePath + ".bkmrk";
				File.WriteAllText(bookmarkFileName, builder.ToString());
			}
			catch
			{
				// Too bad.
			}
		}

		private void RestoreBookmarks()
		{
			string bookmarkFileName = FilePath + ".bkmrk";

			if (!File.Exists(bookmarkFileName))
				return;

			try
			{
				foreach (string line in File.ReadAllLines(bookmarkFileName))
				{
					if (int.TryParse(line, out int lineNumber))
					{
						DocumentLine documentLine = Document.GetLineByNumber(lineNumber);

						if (documentLine != null)
							documentLine.IsBookmarked = true;
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

		public void ApplyErrorsToLines(List<ErrorLine> errorLines)
		{
			foreach (ErrorLine line in errorLines)
			{
				if (line.LineNumber > Document.LineCount)
					continue;

				Document.GetLineByNumber(line.LineNumber).Error = line;
			}
		}

		public void ResetAllErrors()
		{
			foreach (DocumentLine line in Document.Lines)
				line.ClearError();
		}

		private void HandleErrorToolTips(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			DocumentLine hoveredLine = Document.GetLineByOffset(hoveredOffset);

			if (hoveredLine.HasError)
				ShowToolTip("Error:\n" + hoveredLine.Error.Message,
					new SolidColorBrush(Color.FromRgb(128, 96, 96)),
					new SolidColorBrush(Color.FromRgb(96, 64, 64)),
					new SolidColorBrush(Colors.Gainsboro));
		}

		#endregion Error handling

		#region Auto bracket closing

		private void HandleAutoClosing(TextCompositionEventArgs e)
		{
			if (AutoCloseParentheses && e.Text == "(")
				PerformAutoClosing(")");

			if (AutoCloseBraces && e.Text == "{")
				PerformAutoClosing("}");

			if (AutoCloseBrackets && e.Text == "[")
				PerformAutoClosing("]");

			if (AutoCloseQuotes && e.Text == "\"")
				PerformAutoClosing("\"");
		}

		private void PerformAutoClosing(string closingElement)
		{
			SelectedText += closingElement;
			CaretOffset--;

			SelectionStart = CaretOffset;
			SelectionLength = 0;
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
				else
					builder.AppendLine(whitespaceBuilder.ToString());

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
			currentLine.IsBookmarked = !currentLine.IsBookmarked;

			TextArea.TextView.InvalidateLayer(KnownLayer.Background);

			SaveBookmarks();
		}

		public void GoToNextBookmark()
		{
			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);

			for (int i = 1; i < Document.LineCount; i++)
			{
				DocumentLine iLine = Document.GetLineByNumber(i);

				if (iLine.IsBookmarked && iLine.LineNumber > currentLine.LineNumber)
				{
					CaretOffset = iLine.EndOffset;
					ScrollToLine(iLine.LineNumber);
					break;
				}

				if (i == Document.LineCount - 1)
					for (int j = 1; j < Document.LineCount; j++)
					{
						DocumentLine jLine = Document.GetLineByNumber(j);

						if (jLine.IsBookmarked)
						{
							CaretOffset = jLine.EndOffset;
							ScrollToLine(jLine.LineNumber);
							break;
						}
					}
			}
		}

		public void GoToPrevBookmark()
		{
			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);

			for (int i = Document.LineCount - 1; i > 0; i--)
			{
				if (i == 1)
					for (int j = Document.LineCount - 1; j > 0; j--)
					{
						DocumentLine jLine = Document.GetLineByNumber(j);

						if (jLine.IsBookmarked)
						{
							CaretOffset = jLine.EndOffset;
							ScrollToLine(jLine.LineNumber);
							break;
						}
					}

				DocumentLine iLine = Document.GetLineByNumber(i);

				if (iLine.IsBookmarked && iLine.LineNumber < currentLine.LineNumber)
				{
					CaretOffset = iLine.EndOffset;
					ScrollToLine(iLine.LineNumber);
					break;
				}
			}
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
				BorderThickness = new Thickness(2),
				Background = new SolidColorBrush(Color.FromRgb(69, 69, 69)),
				Foreground = Brushes.White,
				BorderBrush = Brushes.Black,
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
				foreach (DocumentLine line in Document.Lines)
					if (line.IsBookmarked)
						line.IsBookmarked = false;

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
			TextViewPosition? position = GetPositionFromPoint(point);

			if (position == null)
				return -1;

			DocumentLine pointLine = Document.GetLineByNumber(((TextViewPosition)position).Line);
			int offset = pointLine.Offset + ((TextViewPosition)position).Column;

			if (offset > Document.TextLength)
				return -1;
			else
				return offset;
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
				new SolidColorBrush(Color.FromRgb(96, 96, 96)),
				new SolidColorBrush(Color.FromRgb(64, 64, 64)),
				new SolidColorBrush(Colors.Gainsboro));

		public void ShowToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
		{
			_specialToolTip.PlacementTarget = this; // Required for property inheritance

			_specialToolTip.BorderBrush = border;
			_specialToolTip.Background = background;
			_specialToolTip.Foreground = foreground;

			_specialToolTip.Content = content;
			_specialToolTip.IsOpen = true;
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
