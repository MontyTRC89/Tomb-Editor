﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Objects;
using TombLib.Scripting.TextEditors.Configs.Bases;
using TombLib.Scripting.TextEditors.Configs.Defaults;
using TombLib.Scripting.TextEditors.Rendering;

namespace TombLib.Scripting.TextEditors.Controls
{
	public class TextEditorBase : TextEditor
	{
		#region Properties, fields and objects

		/* Properties */

		public double DefaultFontSize { get; set; } = TextEditorBaseDefaults.FontSize;

		public bool AutocompleteEnabled { get; set; } = TextEditorBaseDefaults.AutocompleteEnabled;
		public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;

		public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;
		public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;
		public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;
		public bool AutoCloseQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseQuotes;

		public bool ShowDefinitionToolTips { get; set; } = TextEditorBaseDefaults.ShowDefinitionToolTips;

		/// <summary>
		/// Silent session prevents the control from checking if the text has changed, therefore not running any BackgroundWorkers to do so.
		/// <para>Setting this to "true" will also prevent the creation of .backup files.</para>
		/// </summary>
		public bool IsSilentSession { get; protected set; } = false;

		public bool CreateBackupFiles { get; set; } = true;

		public bool IsTextChanged { get; set; } = false;

		public int MinZoom { get; set; } = 25;
		public int MaxZoom { get; set; } = 400;
		public int ZoomStepSize { get; set; } = 15;

		public string CommentPrefix { get; set; } = string.Empty;

		/* Objects */

		public ToolTip SpecialToolTip = new ToolTip();
		public CompletionWindow CompletionWindow = null;

		private BackgroundWorker _contentChangedWorker = new BackgroundWorker();

		private IBackgroundRenderer _bookmarkRenderer;
		private IBackgroundRenderer _errorRenderer;

		#endregion Properties, fields and objects

		#region Construction

		public TextEditorBase()
		{
			InitializeBackgroundWorkers();
			InitializeRenderers();

			BindEventMethods();

			SetNewDefaultSettings();
		}

		private void InitializeBackgroundWorkers()
		{
			_contentChangedWorker.DoWork += ContentChangedWorker_DoWork;
			_contentChangedWorker.RunWorkerCompleted += ContentChangedWorker_RunWorkerCompleted;
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
			TextArea.TextEntered += TextArea_TextEntered;
			TextChanged += TextEditor_TextChanged;

			MouseHover += TextEditor_MouseHover;
			MouseHoverStopped += TextEditor_MouseHoverStopped;

			PreviewMouseWheel += TextEditor_PreviewMouseWheel;
			MouseRightButtonDown += TextEditor_MouseRightButtonDown;

			Unloaded += TextEditor_Unloaded;
		}

		private void SetNewDefaultSettings()
		{
			FontWeight = FontWeights.Normal;

			TextArea.TextView.CurrentLineBackground = new SolidColorBrush(Color.FromArgb(16, 160, 160, 160));
			TextArea.TextView.CurrentLineBorder = new Pen(new SolidColorBrush(Color.FromArgb(24, 192, 192, 192)), 1);

			TextArea.SelectionCornerRadius = 0; // Why does this even exist?

			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
		}

		#endregion Construction

		#region Events

		private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (!IsSilentSession)
			{
				CloseDefinitionToolTip(); // Prevents the ToolTip from covering the screen while typing
				HandleAutoClosing(e);
			}
		}

		private void TextEditor_TextChanged(object sender, EventArgs e)
		{
			if (!IsSilentSession)
			{
				IsTextChanged = true;

				if (!IsUntitledDocument() && !_contentChangedWorker.IsBusy)
					RunContentChangedWorker();
			}
		}

		private void TextEditor_MouseHover(object sender, MouseEventArgs e) =>
			HandleErrorToolTips(e);

		private void TextEditor_MouseHoverStopped(object sender, MouseEventArgs e) =>
			CloseDefinitionToolTip();

		private void TextEditor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
				HandleZoom(e);
		}

		private void TextEditor_MouseRightButtonDown(object sender, MouseButtonEventArgs e) =>
			MoveCaretToMousePosition();

		private void TextEditor_Unloaded(object sender, RoutedEventArgs e) =>
			DeleteBackupFile();

		private void CloseDefinitionToolTip()
		{
			if (SpecialToolTip.IsOpen)
				SpecialToolTip.IsOpen = false;
		}

		private void MoveCaretToMousePosition()
		{
			if (!string.IsNullOrEmpty(SelectedText))
				return;

			TextViewPosition? position = TextArea.TextView.GetPosition(Mouse.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);

			if (position != null)
			{
				SelectionStart = Document.GetOffset(new TextLocation(position.Value.Line, position.Value.Column));
				SelectionLength = 0;
			}
		}

		#endregion Events

		#region File I/O

		public void OpenFile(string filePath, bool silentSession = false)
		{
			Load(filePath);
			Document.FileName = filePath;

			IsTextChanged = false;

			IsSilentSession = silentSession;
		}

		public void SaveCurrentFile() =>
			Save(Document.FileName);

		public new void Save(string fileName)
		{
			base.Save(fileName);

			IsTextChanged = false;

			DeleteBackupFile(); // We don't need the backup file when the original file is saved
		}

		private void DeleteBackupFile()
		{
			string backupFilePath = Document.FileName + ".backup";

			if (File.Exists(backupFilePath))
				File.Delete(backupFilePath);
		}

		public bool IsUntitledDocument()
		{
			return string.IsNullOrEmpty(Document.FileName);
		}

		#endregion File I/O

		#region Content changed handling

		private void RunContentChangedWorker()
		{
			List<object> data = new List<object>
			{
				CreateBackupFiles,
				Document.FileName,
				Text
			};

			_contentChangedWorker.RunWorkerAsync(data);
		}

		private void ContentChangedWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				bool createBackupFiles = (bool)((List<object>)e.Argument)[0];
				string currentFilePath = ((List<object>)e.Argument)[1].ToString();
				string text = ((List<object>)e.Argument)[2].ToString();

				if (createBackupFiles)
				{
					// Create a live backup file so the app can restore lost progress if it crashes
					string backupFilePath = currentFilePath + ".backup";
					File.WriteAllText(backupFilePath, text, Encoding.GetEncoding(1252));
				}

				string fileContent = File.ReadAllText(currentFilePath, Encoding.GetEncoding(1252));

				e.Result = text != fileContent;
			}
			catch
			{
				e.Result = true;
			}
		}

		private void ContentChangedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			IsTextChanged = (bool)e.Result;

			if (!IsTextChanged)
				DeleteBackupFile(); // We don't need the backup file when there are no changes made to the original file
		}

		#endregion Content changed handling

		#region Error handling

		public void ApplyErrorsToLines(List<ErrorLine> errorLines)
		{
			foreach (ErrorLine line in errorLines)
			{
				DocumentLine documentLine = Document.GetLineByNumber(line.LineNumber);

				documentLine.ErrorMessage = line.Message;
				documentLine.ErrorSegmentText = line.ErrorSegmentText;
			}
		}

		public void ResetErrorsOnAllLines()
		{
			foreach (DocumentLine line in Document.Lines)
			{
				line.ErrorMessage = string.Empty;
				line.ErrorSegmentText = string.Empty;
			}
		}

		private void HandleErrorToolTips(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			DocumentLine hoveredLine = Document.GetLineByOffset(hoveredOffset);

			if (!string.IsNullOrEmpty(hoveredLine.ErrorMessage))
				ShowToolTip("Error:\n" + hoveredLine.ErrorMessage,
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

				if (currentLineText.TrimStart().StartsWith(CommentPrefix))
					builder.AppendLine(whitespaceBuilder.ToString() + currentLineText.TrimStart().Remove(0, 1));
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
			get { return _zoom; }
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

		public event EventHandler ZoomChanged;

		protected virtual void OnZoomChanged(EventArgs e) =>
			ZoomChanged?.Invoke(this, e);

		#endregion Zoom

		#region CompletionWindow

		public void InitializeCompletionWindow(int width = 300, int height = 300)
		{
			CompletionWindow = new CompletionWindow(TextArea)
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
			CompletionWindow.Show();
			CompletionWindow.Closed += delegate { CompletionWindow = null; };
		}

		#endregion CompletionWindow

		#region Other public methods

		public void SelectLine(int lineNumber) =>
			SelectLine(Document.GetLineByNumber(lineNumber));

		public void SelectLine(DocumentLine line) =>
			Select(line.Offset, line.Length);

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

		public void ShowToolTip(string content) =>
			ShowToolTip(content,
				new SolidColorBrush(Color.FromRgb(96, 96, 96)),
				new SolidColorBrush(Color.FromRgb(64, 64, 64)),
				new SolidColorBrush(Colors.Gainsboro));

		public void ShowToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
		{
			SpecialToolTip.PlacementTarget = this; // Required for property inheritance

			SpecialToolTip.BorderBrush = border;
			SpecialToolTip.Background = background;
			SpecialToolTip.Foreground = foreground;

			SpecialToolTip.Content = content;
			SpecialToolTip.IsOpen = true;
		}

		public void UpdateSettings(TextEditorConfigurationBase configuration)
		{
			FontSize = configuration.FontSize;
			DefaultFontSize = configuration.FontSize;
			FontFamily = new FontFamily(configuration.FontFamily);

			Document.UndoStack.SizeLimit = configuration.UndoStackSize;

			AutocompleteEnabled = configuration.AutocompleteEnabled;
			LiveErrorUnderlining = configuration.LiveErrorUnderlining;

			AutoCloseParentheses = configuration.AutoCloseParentheses;
			AutoCloseBraces = configuration.AutoCloseBraces;
			AutoCloseBrackets = configuration.AutoCloseBrackets;
			AutoCloseQuotes = configuration.AutoCloseQuotes;

			WordWrap = configuration.WordWrapping;

			Options.HighlightCurrentLine = configuration.HighlightCurrentLine;

			ShowLineNumbers = configuration.ShowLineNumbers;

			Options.ShowSpaces = configuration.ShowVisualSpaces;
			Options.ShowTabs = configuration.ShowVisualTabs;

			ShowDefinitionToolTips = configuration.ShowDefinitionToolTips;
		}

		#endregion Other public methods
	}
}
