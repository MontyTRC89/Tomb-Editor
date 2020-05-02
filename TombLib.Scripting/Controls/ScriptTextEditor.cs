using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using TombLib.Scripting.Autocomplete;
using TombLib.Scripting.ErrorChecking;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Rendering;
using TombLib.Scripting.Resources;
using TombLib.Scripting.Resources.ToolTips;
using TombLib.Scripting.SyntaxTidying;

namespace TombLib.Scripting.Controls
{
	public class ScriptTextEditor : BaseTextEditor
	{
		#region Properties, fields and objects

		public bool ShowSectionSeparators
		{
			get { return _showSectionSeparators; }
			set
			{
				_showSectionSeparators = value;

				if (_showSectionSeparators)
				{
					if (!TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
						TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
				}
				else
				{
					if (TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
						TextArea.TextView.BackgroundRenderers.Remove(_sectionRenderer);
				}

				TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
			}
		}

		private bool _showSectionSeparators;

		private BackgroundWorker _autocompleteWorker = new BackgroundWorker();
		private BackgroundWorker _errorDetectionWorker = new BackgroundWorker();

		private DispatcherTimer _errorUpdateTimer = new DispatcherTimer();

		private IBackgroundRenderer _sectionRenderer;

		#endregion Properties, fields and objects

		#region Construction

		public ScriptTextEditor()
		{
			InitializeBackgroundWorkers();
			InitializeTimers();
			InitializeRenderers();

			BindEventMethods();

			SyntaxHighlighting = new ScriptSyntaxHighlighting();

			if (ShowSectionSeparators)
				TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
		}

		private void InitializeBackgroundWorkers()
		{
			_autocompleteWorker.DoWork += AutocompleteWorker_DoWork;
			_autocompleteWorker.RunWorkerCompleted += AutocompleteWorker_RunWorkerCompleted;

			_errorDetectionWorker.DoWork += ErrorWorker_DoWork;
			_errorDetectionWorker.RunWorkerCompleted += ErrorWorker_RunWorkerCompleted;
		}

		private void InitializeTimers()
		{
			_errorUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
			_errorUpdateTimer.Tick += ErrorUpdateTimer_Tick;
		}

		private void InitializeRenderers()
		{
			_sectionRenderer = new SectionRenderer(this);
		}

		private void BindEventMethods()
		{
			TextArea.TextEntered += TextArea_TextEntered;
			TextChanged += TextBox_TextChanged;

			MouseHover += TextBox_MouseHover;
		}

		#endregion Construction

		#region Events

		private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (!IsSilentSession && AutocompleteEnabled)
				HandleAutocomplete(e);
		}

		private void TextBox_TextChanged(object sender, EventArgs e)
		{
			if (!IsSilentSession && LiveErrorUnderlining)
				ResetErrorUpdateTimer();
		}

		private void TextBox_MouseHover(object sender, MouseEventArgs e) =>
			HandleDefinitionToolTips(e);

		#endregion Events

		#region Autocomplete

		private void HandleAutocomplete(TextCompositionEventArgs e)
		{
			if (CompletionWindow == null) // Prevent window duplicates
			{
				if (e.Text == " " && CaretOffset > 1)
					HandleAutocompleteAfterSpace();
				else if (e.Text == "_" && CaretOffset > 1)
					HandleAutocompleteAfterUnderscore();
				else if (Document.GetLineByOffset(CaretOffset).Length == 1)
					HandleAutocompleteOnEmptyLine();
			}
		}

		private void HandleAutocompleteAfterSpace()
		{
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

		private void HandleAutocompleteAfterUnderscore()
		{
			if (Document.GetCharAt(CaretOffset - 2) == '_')
			{
				int wordStartOffset =
					TextUtilities.GetNextCaretPosition(Document, CaretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

				string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

				InitializeCompletionWindow();
				CompletionWindow.StartOffset = wordStartOffset;

				foreach (string mnemonicConstant in ScriptKeyWords.AllMnemonics)
				{
					if (mnemonicConstant.StartsWith(word, StringComparison.OrdinalIgnoreCase))
						CompletionWindow.CompletionList.CompletionData.Add(new CompletionData(mnemonicConstant));
				}

				ShowCompletionWindow();
			}
		}

		private void HandleAutocompleteOnEmptyLine()
		{
			InitializeCompletionWindow();
			CompletionWindow.StartOffset--;

			foreach (ICompletionData item in ScriptAutocomplete.GetNewLineAutocompleteList())
				CompletionWindow.CompletionList.CompletionData.Add(item);

			ShowCompletionWindow();
		}

		private void AutocompleteWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<object> data = (List<object>)e.Argument;

			TextDocument document = new TextDocument(data[0].ToString());
			int caretOffset = (int)data[1];

			e.Result = ScriptAutocomplete.GetCompletionData(document, caretOffset);
		}

		private void AutocompleteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			List<ICompletionData> completionData = (List<ICompletionData>)e.Result;

			if (completionData == null)
				return;

			if (completionData.Count == 0)
				return;

			InitializeCompletionWindow();

			int wordStartOffset =
				TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

			if (!word.StartsWith("=") && !word.StartsWith(","))
				CompletionWindow.StartOffset = wordStartOffset;

			foreach (ICompletionData item in completionData)
				CompletionWindow.CompletionList.CompletionData.Add(item);

			ShowCompletionWindow();
		}

		#endregion Autocomplete

		#region Error handling

		public void ManuallyCheckForErrors() =>
			CheckForErrors();

		private void ResetErrorUpdateTimer()
		{
			_errorUpdateTimer.Stop();
			_errorUpdateTimer.Start();
		}

		private void ErrorUpdateTimer_Tick(object sender, EventArgs e)
		{
			CheckForErrors();

			_errorUpdateTimer.Stop();
		}

		private void ErrorWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			e.Result = ScriptErrorChecking.GetErrorLines(new TextDocument(e.Argument.ToString()));
		}

		private void ErrorWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result == null)
				return;

			ResetErrorsOnAllLines();
			ApplyErrorsToLines((List<ErrorLine>)e.Result);

			TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		}

		private void CheckForErrors()
		{
			if (!_errorDetectionWorker.IsBusy)
				_errorDetectionWorker.RunWorkerAsync(Text);
		}

		#endregion Error handling

		#region ToolTips

		private void HandleDefinitionToolTips(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			string hoveredWord = GetWordFromOffset(hoveredOffset);

			if (string.IsNullOrEmpty(hoveredWord))
				return;

			hoveredWord = hoveredWord.Trim('[').Trim(']').Trim('=').Trim(',');

			// Check if toolTips are enabled and if the hovered word is not just whitespace
			if (ShowDefinitionToolTips && !string.IsNullOrWhiteSpace(hoveredWord))
			{
				DocumentLine hoveredLine = Document.GetLineByOffset(hoveredOffset);
				string hoveredLineText = Document.GetText(hoveredLine.Offset, hoveredLine.Length);

				if (hoveredLineText.StartsWith("[")) // If the word is a section header
					ShowSectionToolTip(hoveredWord);
				else
					ShowCommandToolTip(hoveredWord, hoveredLine.LineNumber);
			}
		}

		private void ShowSectionToolTip(string hoveredWord)
		{
			// Get resources from SectionToolTips.resx
			ResourceManager sectionToolTipResource = new ResourceManager(typeof(SectionToolTips));
			ResourceSet resourceSet = sectionToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Check if the hovered word exists in the file, if so, display the toolTip
			foreach (DictionaryEntry entry in resourceSet)
				if (entry.Key.ToString().Equals(hoveredWord, StringComparison.OrdinalIgnoreCase))
				{
					string content = "[" + entry.Key.ToString() + "]\n\n" + entry.Value.ToString();
					ShowDefinitionToolTip(content);
					return;
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
				// Check if the hovered word exists in the file, if so, display the toolTip
				foreach (DictionaryEntry entry in resourceSet)
					if (hoveredWord == entry.Key.ToString())
					{
						string content = entry.Key.ToString() + "=\n\n" + entry.Value.ToString();
						ShowDefinitionToolTip(content);
						return;
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
					ShowDefinitionToolTip(content);
					return;
				}

				i--; // Go 1 line higher if no section header was found yet
			}
			while (!Document.GetText(Document.GetLineByNumber(i + 1).Offset, Document.GetLineByNumber(i + 1).Length).StartsWith("["));
		}

		#endregion ToolTips

		#region Other public methods

		public void TidyDocument(bool trimOnly = false)
		{
			Vector scrollOffset = TextArea.TextView.ScrollOffset;

			SelectAll();
			SelectedText = trimOnly ? SharedTidyingMethods.TrimEndingWhitespace(Text) : ScriptTidying.ReindentScript(Text);
			Select(0, 0);

			ScrollToHorizontalOffset(scrollOffset.X);
			ScrollToVerticalOffset(scrollOffset.Y);
		}

		#endregion Other public methods
	}
}
