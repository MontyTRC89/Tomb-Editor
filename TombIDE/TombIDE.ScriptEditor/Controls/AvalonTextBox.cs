using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TombIDE.ScriptEditor.Objects;
using TombIDE.ScriptEditor.Rendering;
using TombIDE.ScriptEditor.Resources;
using TombIDE.ScriptEditor.Resources.ToolTips;

namespace TombIDE.ScriptEditor.Controls
{
	public class AvalonTextBox : TextEditor
	{
		/* Properties */

		public bool IsTextChanged { get; set; } = false;

		public double DefaultFontSize { get; set; } = 16d;
		public bool AutocompleteEnabled { get; set; } = true;
		public bool LiveErrorChecking { get; set; } = true;
		public bool AutoCloseBrackets { get; set; } = true;
		public bool AutoCloseQuotes { get; set; } = true;
		public bool ShowToolTips { get; set; } = true;

		/* Fields */

		public List<int> BookmarkedLines = new List<int>();
		public List<ErrorLine> ErrorLines = new List<ErrorLine>();

		/* Objects */

		private BackgroundWorker _autocompleteWorker = new BackgroundWorker();
		private BackgroundWorker _errorDetectionWorker = new BackgroundWorker();
		private BackgroundWorker _contentChangedWorker = new BackgroundWorker();
		private DispatcherTimer _errorUpdateTimer = new DispatcherTimer();
		private CompletionWindow _completionWindow = null;
		private ToolTip _definitionToolTip = new ToolTip();

		#region Initialization

		public AvalonTextBox()
		{
			SetNewDefaultSettings();

			// Bind event methods
			Document.PropertyChanged += Document_PropertyChanged;
			TextArea.TextEntered += TextArea_TextEntered;
			TextChanged += AvalonTextBox_TextChanged;
			MouseHover += AvalonTextBox_MouseHover;
			MouseHoverStopped += AvalonTextBox_MouseHoverStopped;
			PreviewMouseWheel += AvalonTextBox_PreviewMouseWheel;
			MouseRightButtonDown += AvalonTextBox_MouseRightButtonDown;

			// Initialize background workers
			_autocompleteWorker.DoWork += AutocompleteWorker_DoWork;
			_autocompleteWorker.RunWorkerCompleted += AutocompleteWorker_RunWorkerCompleted;

			_errorDetectionWorker.DoWork += ErrorWorker_DoWork;
			_errorDetectionWorker.RunWorkerCompleted += ErrorWorker_RunWorkerCompleted;

			_contentChangedWorker.DoWork += ContentChangedWorker_DoWork;
			_contentChangedWorker.RunWorkerCompleted += ContentChangedWorker_RunWorkerCompleted;

			// Initialize timers
			_errorUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1);
			_errorUpdateTimer.Tick += ErrorUpdateTimer_Tick;
		}

		private void SetNewDefaultSettings()
		{
			FontFamily = new FontFamily("Consolas");
			FontSize = 16d;

			SyntaxHighlighting = new ScriptSyntaxHighlighting();

			TextArea.Options.HighlightCurrentLine = true;
			TextArea.TextView.CurrentLineBackground = new SolidColorBrush(Color.FromArgb(20, 128, 128, 128));
			TextArea.TextView.CurrentLineBorder = new Pen(new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)), 1);

			TextArea.SelectionCornerRadius = 0; // Why does this even exist?

			TextArea.TextView.BackgroundRenderers.Add(new BookmarkRenderer(this));
			TextArea.TextView.BackgroundRenderers.Add(new ErrorRenderer(this));

			TextArea.Caret.CaretBrush = Brushes.White;
			Background = new SolidColorBrush(Color.FromRgb(32, 32, 32));
			Foreground = new SolidColorBrush(Colors.LightSalmon);

			ShowLineNumbers = true;

			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
		}

		#endregion Initialization

		#region Text events

		private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "LineCount")
				ErrorLines.Clear(); // Prevents graphical glitches
		}

		private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (_definitionToolTip.IsOpen)
				_definitionToolTip.IsOpen = false; // Prevents the ToolTip from covering the screen while typing

			/* Auto closing */

			if (AutoCloseBrackets && e.Text == "[")
				CloseBrackets();

			if (AutoCloseQuotes && e.Text == "\"")
				CloseQuotes();

			/* Autocomplete */

			HandleAutocomplete(e);
		}

		private void AvalonTextBox_TextChanged(object sender, EventArgs e)
		{
			IsTextChanged = true;

			// Remove error highlighting from the currently modified line (if the current line exists on the ErrorLines list ofc.)
			foreach (ErrorLine line in ErrorLines)
			{
				if (line.LineNumber == Document.GetLineByOffset(CaretOffset).LineNumber)
				{
					ErrorLines.Remove(line);
					break;
				}
			}

			if (LiveErrorChecking)
			{
				// Reset the error update timer, because we want it to trigger when the user is not typing anything
				_errorUpdateTimer.Stop();
				_errorUpdateTimer.Start();
			}

			if (!string.IsNullOrEmpty(Document.FileName)) // If the currently modified file is not "Untitled"
			{
				List<string> data = new List<string>
				{
					Document.FileName,
					Text
				};

				if (!_contentChangedWorker.IsBusy)
					_contentChangedWorker.RunWorkerAsync(data);
			}
		}

		private void CloseBrackets()
		{
			SelectedText += "]";
			CaretOffset--;

			SelectionStart = CaretOffset;
			SelectionLength = 0;
		}

		private void CloseQuotes()
		{
			SelectedText += "\"";
			CaretOffset--;

			SelectionStart = CaretOffset;
			SelectionLength = 0;
		}

		#endregion Text events

		#region Autocomplete

		private void HandleAutocomplete(TextCompositionEventArgs e)
		{
			if (_completionWindow != null)
				return;

			if (e.Text == " ")
			{
				if (CaretOffset < 2)
					return;

				if (Document.GetCharAt(CaretOffset - 2) == '=' || Document.GetCharAt(CaretOffset - 2) == ',' && !_autocompleteWorker.IsBusy)
				{
					List<object> data = new List<object>
					{
						Text,
						CaretOffset
					};

					_autocompleteWorker.RunWorkerAsync(data);
				}
			}
			else if (e.Text == "_")
			{
				if (CaretOffset < 2)
					return;

				if (Document.GetCharAt(CaretOffset - 2) == '_')
				{
					int wordStart = TextUtilities.GetNextCaretPosition(Document, CaretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

					string word = Document.GetText(wordStart, CaretOffset - wordStart);

					InitializeCompletionWindow();
					_completionWindow.StartOffset = wordStart;

					IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

					foreach (string mnemonicConstant in KeyWords.AllMnemonics)
					{
						if (mnemonicConstant.StartsWith(word, StringComparison.OrdinalIgnoreCase))
							data.Add(new CompletionData(mnemonicConstant));
					}

					ShowCompletionWindow();
				}
			}
			else if (Document.GetLineByOffset(CaretOffset).Length == 1)
			{
				InitializeCompletionWindow();
				_completionWindow.StartOffset -= 1;

				IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

				foreach (ICompletionData item in Autocomplete.GetNewLineAutocompleteList())
					data.Add(item);

				ShowCompletionWindow();
			}
		}

		private void InitializeCompletionWindow()
		{
			_completionWindow = new CompletionWindow(TextArea)
			{
				WindowStyle = WindowStyle.None,
				ResizeMode = ResizeMode.NoResize,
				BorderThickness = new Thickness(0),
				Width = 300,
				Height = 300
			};
		}

		private void ShowCompletionWindow()
		{
			_completionWindow.Show();

			_completionWindow.Closed += delegate { _completionWindow = null; };
		}

		private void AutocompleteWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<object> data = (List<object>)e.Argument;

			string text = (string)data[0];
			int caretOffset = (int)data[1];

			TextDocument document = new TextDocument(text);
			List<ICompletionData> completionData = null;

			DocumentLine currentLine = document.GetLineByOffset(caretOffset);
			string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

			if (Regex.IsMatch(currentLineText, @"Customize\s*?=.*?,", RegexOptions.IgnoreCase))
				completionData = Autocomplete.GetCustomizeAutocompleteList(text, caretOffset);
			else if (Regex.IsMatch(currentLineText, @"Parameters\s*?=.*?,", RegexOptions.IgnoreCase))
				completionData = Autocomplete.GetParametersAutocompleteList(text, caretOffset);
			else
				completionData = Autocomplete.GetCommandAutocompleteList(text, caretOffset);

			e.Result = completionData;
		}

		private void AutocompleteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			List<ICompletionData> completionData = (List<ICompletionData>)e.Result;

			if (completionData == null)
				return;

			if (completionData.Count == 0)
				return;

			InitializeCompletionWindow();

			int wordStart = TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			string word = Document.GetText(wordStart, CaretOffset - wordStart);

			if (!word.StartsWith("=") && !word.StartsWith(","))
				_completionWindow.StartOffset = wordStart;

			IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

			foreach (ICompletionData item in completionData)
				data.Add(item);

			ShowCompletionWindow();
		}

		#endregion Autocomplete

		#region Errors

		private void ErrorUpdateTimer_Tick(object sender, EventArgs e)
		{
			if (!_errorDetectionWorker.IsBusy)
				_errorDetectionWorker.RunWorkerAsync(Text);

			_errorUpdateTimer.Stop();
		}

		private void ErrorWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			e.Result = ErrorChecking.GetErrorLines(e.Argument.ToString());
		}

		private void ErrorWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result == null)
				return;

			ErrorLines = (List<ErrorLine>)e.Result;
		}

		#endregion Errors

		#region ToolTips

		private void AvalonTextBox_MouseHoverStopped(object sender, MouseEventArgs e)
		{
			_definitionToolTip.IsOpen = false;
		}

		private void AvalonTextBox_MouseHover(object sender, MouseEventArgs e)
		{
			TextViewPosition? position = GetPositionFromPoint(e.GetPosition(this));

			if (position != null)
			{
				int currentLineNumber = ((TextViewPosition)position).Line;

				foreach (ErrorLine errorLine in ErrorLines)
				{
					if (errorLine.LineNumber == currentLineNumber)
					{
						_definitionToolTip.PlacementTarget = this;

						_definitionToolTip.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
						_definitionToolTip.Foreground = new SolidColorBrush(Colors.Gainsboro);

						_definitionToolTip.Content = errorLine.Message;
						_definitionToolTip.IsOpen = true;

						e.Handled = true;

						return;
					}
				}

				int hoverOffset = Document.GetLineByNumber(currentLineNumber).Offset + ((TextViewPosition)position).Column;

				int wordStart = TextUtilities.GetNextCaretPosition(Document, hoverOffset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);
				int wordEnd = TextUtilities.GetNextCaretPosition(Document, hoverOffset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);

				if (wordStart > -1 && wordEnd > -1)
				{
					string hoveredWord = Document.GetText(wordStart, wordEnd - wordStart).Trim().Trim('[').Trim(']').Trim('=').Trim(',');

					// Check if toolTips are enabled and if the hovered text is not just whitespace
					if (ShowToolTips && !string.IsNullOrWhiteSpace(hoveredWord))
					{
						DocumentLine currentLine = Document.GetLineByOffset(wordStart);

						if (Document.GetText(currentLine.Offset, currentLine.Length).StartsWith("[")) // If the word is a section header
							ShowSectionToolTip(hoveredWord);
						else
							ShowCommandToolTip(hoveredWord, currentLineNumber);

						e.Handled = true;
					}
				}
			}
		}

		private void ShowSectionToolTip(string hoveredWord)
		{
			// Get resources from SectionToolTips.resx
			ResourceManager sectionToolTipResource = new ResourceManager(typeof(SectionToolTips));
			ResourceSet resourceSet = sectionToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Check if the hovered word exists in the file, if so, display the toolTip
			foreach (DictionaryEntry entry in resourceSet)
			{
				if (hoveredWord == entry.Key.ToString())
				{
					string content = "[" + entry.Key.ToString() + "]\n\n" + entry.Value.ToString();

					_definitionToolTip.PlacementTarget = this; // Required for property inheritance

					_definitionToolTip.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
					_definitionToolTip.Foreground = new SolidColorBrush(Colors.Gainsboro);

					_definitionToolTip.Content = content;
					_definitionToolTip.IsOpen = true;

					return;
				}
			}
		}

		private void ShowCommandToolTip(string hoveredWord, int currentLineNumber)
		{
			// Get resources from CommandToolTips.resx
			ResourceManager commandToolTipResource = new ResourceManager(typeof(CommandToolTips));
			ResourceSet resourceSet = commandToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			if (hoveredWord.ToLower() == "level")
				HandleLevelToolTip(currentLineNumber); // There are different definitions for the "Level" command, so handle them all
			else
			{
				// Check if the hovered word exists in the file, if so, display the toolTip
				foreach (DictionaryEntry entry in resourceSet)
				{
					if (hoveredWord == entry.Key.ToString())
					{
						string content = entry.Key.ToString() + "=\n\n" + entry.Value.ToString();

						_definitionToolTip.PlacementTarget = this; // Required for property inheritance

						_definitionToolTip.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
						_definitionToolTip.Foreground = new SolidColorBrush(Colors.Gainsboro);

						_definitionToolTip.Content = content;
						_definitionToolTip.IsOpen = true;

						return;
					}
				}
			}
		}

		private void HandleLevelToolTip(int currentLineNumber)
		{
			int i = currentLineNumber;

			do
			{
				if (i < 0)
					return; // The line number might go to -1 and it will crash the app, so stop the loop to prevent it

				DocumentLine line = Document.GetLineByNumber(i);
				string lineText = Document.GetText(line.Offset, line.Length);

				string content = string.Empty;

				if (lineText.StartsWith("[PSXExtensions]", StringComparison.OrdinalIgnoreCase))
					content = "[PSXExtensions] Level=\n\n" + CommandToolTips.LevelPSX;
				else if (lineText.StartsWith("[PCExtensions]", StringComparison.OrdinalIgnoreCase))
					content = "[PCExtensions] Level=\n\n" + CommandToolTips.LevelPC;
				else if (lineText.StartsWith("[Title]", StringComparison.OrdinalIgnoreCase))
					content = "[Title] Level=\n\n" + CommandToolTips.LevelTitle;
				else if (lineText.StartsWith("[Level]", StringComparison.OrdinalIgnoreCase))
					content = "Level=\n\n" + CommandToolTips.LevelLevel;

				if (!string.IsNullOrEmpty(content))
				{
					_definitionToolTip.PlacementTarget = this; // Required for property inheritance

					_definitionToolTip.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
					_definitionToolTip.Foreground = new SolidColorBrush(Colors.Gainsboro);

					_definitionToolTip.Content = content;
					_definitionToolTip.IsOpen = true;

					return;
				}

				i--; // Go 1 line higher if no section header was found yet
			}
			while (!Document.GetText(Document.GetLineByNumber(i + 1).Offset, Document.GetLineByNumber(i + 1).Length).StartsWith("["));
		}

		#endregion ToolTips

		#region Commenting

		public void CommentLines()
		{
			DocumentLine startLine = Document.GetLineByOffset(SelectionStart);
			DocumentLine endLine = Document.GetLineByOffset(SelectionStart + SelectionLength);

			int totalLineLength = 0;

			StringBuilder builder = new StringBuilder();

			for (int i = startLine.LineNumber; i <= endLine.LineNumber; i++)
			{
				DocumentLine currentLine = Document.GetLineByNumber(i);
				string currentLineText = Document.GetText(currentLine.Offset, currentLine.Length);

				StringBuilder whitespaceBuilder = new StringBuilder();

				for (int j = 0; j < currentLineText.Length; j++)
				{
					char c = currentLineText[j];

					if (char.IsWhiteSpace(c))
						whitespaceBuilder.Append(c);
					else
						break;
				}

				if (!string.IsNullOrWhiteSpace(currentLineText))
					builder.AppendLine(whitespaceBuilder.ToString() + ";" + currentLineText.TrimStart());
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

			StringBuilder builder = new StringBuilder();

			for (int i = startLine.LineNumber; i <= endLine.LineNumber; i++)
			{
				DocumentLine currentLine = Document.GetLineByNumber(i);
				string currentLineText = Document.GetText(currentLine.Offset, currentLine.Length);

				StringBuilder whitespaceBuilder = new StringBuilder();

				for (int j = 0; j < currentLineText.Length; j++)
				{
					char c = currentLineText[j];

					if (char.IsWhiteSpace(c))
						whitespaceBuilder.Append(c);
					else
						break;
				}

				if (currentLineText.TrimStart().StartsWith(";"))
					builder.AppendLine(whitespaceBuilder.ToString() + currentLineText.TrimStart().Remove(0, 1));
				else
					builder.AppendLine(currentLineText);

				totalLineLength += currentLine.TotalLength;
			}

			Select(startLine.Offset, totalLineLength);
			SelectedText = builder.ToString();

			Select(startLine.Offset, SelectionLength - 1);
		}

		#endregion Commenting

		#region Zoom

		private int _zoom = 100;

		public int Zoom
		{
			get { return _zoom; }
			set
			{
				FontSize = DefaultFontSize * value / 100;
				_zoom = value;
			}
		}

		private void AvalonTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (e.Delta > 0) // Increase
				{
					if (_zoom < 400)
					{
						_zoom += 15;
						FontSize = DefaultFontSize * _zoom / 100;
					}
				}
				else // Decrease
				{
					if (_zoom > 25)
					{
						_zoom -= 15;
						FontSize = DefaultFontSize * _zoom / 100;
					}
				}

				OnZoomChanged(EventArgs.Empty);
				e.Handled = true;
			}
		}

		public event EventHandler ZoomChanged;

		protected virtual void OnZoomChanged(EventArgs e)
		{
			ZoomChanged?.Invoke(this, e);
		}

		#endregion Zoom

		#region Bookmarks

		public void ToggleBookmark()
		{
			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);

			bool lineExists = false;

			foreach (int lineNumber in BookmarkedLines)
			{
				if (lineNumber == currentLine.LineNumber)
				{
					BookmarkedLines.Remove(lineNumber);
					lineExists = true;
					break;
				}
			}

			if (!lineExists)
				BookmarkedLines.Add(currentLine.LineNumber);

			BookmarkedLines.Sort(delegate (int l1, int l2) { return l1.CompareTo(l2); });
		}

		public void GoToNextBookmark()
		{
			if (BookmarkedLines.Count == 0)
				return;

			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);

			for (int i = 0; i <= BookmarkedLines.Count; i++)
			{
				if (i == BookmarkedLines.Count)
				{
					CaretOffset = Document.GetLineByNumber(BookmarkedLines.First()).EndOffset;
					ScrollToLine(BookmarkedLines.First());
					break;
				}

				DocumentLine line = Document.GetLineByNumber(BookmarkedLines[i]);

				if (line.LineNumber > currentLine.LineNumber)
				{
					CaretOffset = line.EndOffset;
					ScrollToLine(line.LineNumber);
					break;
				}
			}
		}

		public void GoToPrevBookmark()
		{
			if (BookmarkedLines.Count == 0)
				return;

			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);

			for (int i = BookmarkedLines.Count - 1; i >= -1; i--)
			{
				if (i == -1)
				{
					CaretOffset = Document.GetLineByNumber(BookmarkedLines.Last()).EndOffset;
					ScrollToLine(BookmarkedLines.Last());
					break;
				}

				DocumentLine line = Document.GetLineByNumber(BookmarkedLines[i]);

				if (line.LineNumber < currentLine.LineNumber)
				{
					CaretOffset = line.EndOffset;
					ScrollToLine(line.LineNumber);
					break;
				}
			}
		}

		#endregion Bookmarks

		private void ContentChangedWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			string currentFilePath = ((List<string>)e.Argument)[0];
			string text = ((List<string>)e.Argument)[1];

			// Create a live backup file so the app can restore lost progress if it crashes
			string backupFilePath = currentFilePath + ".backup";
			File.WriteAllText(backupFilePath, text, Encoding.GetEncoding(1252));

			string fileContent = File.ReadAllText(currentFilePath, Encoding.GetEncoding(1252));

			if (text == fileContent) // If the editor content is identical to the file content
				e.Result = false;
			else
				e.Result = true;
		}

		private void ContentChangedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			IsTextChanged = (bool)e.Result;

			if (!IsTextChanged)
			{
				string backupFilePath = Document.FileName + ".backup";

				if (File.Exists(backupFilePath))
					File.Delete(backupFilePath); // We don't need the backup file when there are no changes made to the original one
			}
		}

		public void OpenFile(string filePath)
		{
			Load(filePath);
			Document.FileName = filePath;

			IsTextChanged = false;
		}

		public void SaveCurrentFile()
		{
			Save(Document.FileName);
		}

		public new void Save(string fileName)
		{
			base.Save(fileName);

			IsTextChanged = false;

			string backupFilePath = Document.FileName + ".backup";

			if (File.Exists(backupFilePath))
				File.Delete(backupFilePath); // We don't need the backup file when the original file is saved
		}

		public void TidyDocument(bool trimOnly = false)
		{
			Vector scrollOffset = TextArea.TextView.ScrollOffset;

			SelectAll();
			SelectedText = trimOnly ? SyntaxTidying.TrimScript(Text) : SyntaxTidying.ReindentScript(Text);
			Select(0, 0);

			ScrollToHorizontalOffset(scrollOffset.X);
			ScrollToVerticalOffset(scrollOffset.Y);
		}

		public void CheckForErrors()
		{
			if (!_errorDetectionWorker.IsBusy)
				_errorDetectionWorker.RunWorkerAsync(Text);
		}

		private void AvalonTextBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			TextViewPosition? position = TextArea.TextView.GetPosition(Mouse.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);

			if (position != null)
			{
				SelectionStart = Document.GetOffset(new TextLocation(position.Value.Line, position.Value.Column));
				SelectionLength = 0;
			}
		}
	}
}
