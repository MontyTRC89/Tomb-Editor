using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.ClassicScript.Utils;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Workers;

namespace TombLib.Scripting.ClassicScript
{
	public sealed class ClassicScriptEditor : TextEditorBase
	{
		#region Properties

		public CodeCleaner Cleaner { get; set; } = new CodeCleaner();

		private bool _showSectionSeparators;
		public bool ShowSectionSeparators
		{
			get => _showSectionSeparators;
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

		#endregion Properties

		#region Fields

		private BackgroundWorker _autocompleteWorker;
		private ErrorDetectionWorker _errorDetectionWorker;

		private IBackgroundRenderer _sectionRenderer;

		#endregion Fields

		#region Construction

		public ClassicScriptEditor()
		{
			InitializeBackgroundWorkers();
			InitializeRenderers();

			BindEventMethods();

			CommentPrefix = ";";
		}

		private void InitializeBackgroundWorkers()
		{
			_autocompleteWorker = new BackgroundWorker();
			_autocompleteWorker.DoWork += AutocompleteWorker_DoWork;
			_autocompleteWorker.RunWorkerCompleted += AutocompleteWorker_RunWorkerCompleted;

			_errorDetectionWorker = new ErrorDetectionWorker(new ErrorDetector(), new TimeSpan(500));
			_errorDetectionWorker.RunWorkerCompleted += ErrorWorker_RunWorkerCompleted;
		}

		private void InitializeRenderers()
		{
			_sectionRenderer = new SectionRenderer(this);

			if (ShowSectionSeparators)
				TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
		}

		private void BindEventMethods()
		{
			TextArea.TextEntered += TextEditor_TextEntered;
			TextChanged += TextEditor_TextChanged;

			KeyDown += TextEditor_KeyDown;
			MouseHover += TextEditor_MouseHover;
		}

		#endregion Construction

		#region Events

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled)
				HandleAutocomplete(e);
		}

		private void TextEditor_TextChanged(object sender, EventArgs e)
		{
			if (LiveErrorUnderlining)
				_errorDetectionWorker.RunErrorCheckOnIdle(Text);
		}

		private void TextEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F1)
				InputFreeIndex();
			else if (e.Key == Key.F12)
			{
				if (_specialToolTip.IsOpen && HoveredWordArgs != null)
				{
					OnWordDefinitionRequested(HoveredWordArgs);
					HoveredWordArgs = null;
				}
				else
				{
					string word = WordParser.GetWordFromOffset(Document, CaretOffset);
					WordType type = WordParser.GetWordTypeFromOffset(Document, CaretOffset);

					if (!string.IsNullOrEmpty(word) && type != WordType.Unknown)
						OnWordDefinitionRequested(new WordDefinitionEventArgs(word, type));
				}
			}
		}

		private void TextEditor_MouseHover(object sender, MouseEventArgs e)
			=> HandleDefinitionToolTips(e);

		#endregion Events

		#region Autocomplete

		// TODO: Recheck

		private void HandleAutocomplete(TextCompositionEventArgs e)
		{
			if (_completionWindow == null) // Prevents window duplicates
			{
				if (e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					HandleAutocompleteAfterSpaceCtrl();
				else if (Document.GetLineByOffset(CaretOffset).Length == 1)
					HandleAutocompleteOnEmptyLine();
				else if (e.Text != "_" && CaretOffset > 1)
				{
					string firstLetterOfLastFlag = GetFirstLetterOfLastFlag();
					string firstLetterOfCurrentFlag = GetFirstLetterOfCurrentArgument();

					if (!string.IsNullOrEmpty(firstLetterOfLastFlag)
						&& e.Text.Equals(firstLetterOfLastFlag, StringComparison.OrdinalIgnoreCase)
						&& CaretOffset > 1)
					{
						if (!string.IsNullOrEmpty(firstLetterOfCurrentFlag)
							&& firstLetterOfCurrentFlag.Equals(firstLetterOfLastFlag, StringComparison.OrdinalIgnoreCase))
							HandleAutocompleteAfterSpace();
						else
							HandleAutocompleteForNextFlag();
					}
					else
						HandleAutocompleteAfterSpace();
				}
				else if (e.Text == "_" && CaretOffset > 1)
					HandleAutocompleteAfterUnderscore();
			}
		}

		private string GetFirstLetterOfCurrentArgument()
		{
			try // TODO: Possibly get rid of this try / catch
			{
				int currentArgumentIndex = ArgumentParser.GetArgumentIndexAtOffset(Document, CaretOffset);

				if (currentArgumentIndex == -1)
					return null;

				string syntax = CommandParser.GetCommandSyntax(Document, CaretOffset);

				if (string.IsNullOrEmpty(syntax))
					return null;

				string[] syntaxArguments = syntax.Split(',');

				if (syntaxArguments.Length < currentArgumentIndex)
					return null;

				string currentSyntaxArgument = syntaxArguments[currentArgumentIndex];

				if (!currentSyntaxArgument.Contains("_"))
					return null;

				string flagPrefix = currentSyntaxArgument.Split('_')[0].Split('(')[1];

				return flagPrefix[0].ToString();
			}
			catch
			{
				return null;
			}
		}

		private string GetFirstLetterOfLastFlag()
		{
			int currentArgumentIndex = ArgumentParser.GetArgumentIndexAtOffset(Document, CaretOffset);

			if (currentArgumentIndex == -1 || currentArgumentIndex == 0)
				return null;

			string prevArgument = ArgumentParser.GetArgumentFromIndex(Document, CaretOffset, currentArgumentIndex - 1).Trim();

			if (!prevArgument.Contains("_"))
				return null;

			if (prevArgument.Contains("="))
				prevArgument.Split('=').Last().Trim();

			return prevArgument[0].ToString();
		}

		private void HandleAutocompleteAfterSpaceCtrl()
		{
			Select(CaretOffset - 1, 1);
			SelectedText = string.Empty;

			string wholeLineText = CommandParser.GetWholeCommandLineText(Document, CaretOffset);

			if (string.IsNullOrEmpty(wholeLineText))
				HandleAutocompleteOnEmptyLine();
			else if (!_autocompleteWorker.IsBusy)
			{
				var data = new List<object>
				{
					Text,
					CaretOffset,
					-1
				};

				_autocompleteWorker.RunWorkerAsync(data);
			}
		}

		private void HandleAutocompleteAfterSpace()
		{
			if ((Document.GetCharAt(CaretOffset - 2) == '='
				|| Document.GetCharAt(CaretOffset - 2) == ','
				|| Document.GetCharAt(CaretOffset - 2) == '_')
				&& !_autocompleteWorker.IsBusy)
			{
				var data = new List<object>
				{
					Text,
					CaretOffset,
					-1
				};

				_autocompleteWorker.RunWorkerAsync(data);
			}
		}

		private void HandleAutocompleteAfterUnderscore()
		{
			if (Document.GetCharAt(CaretOffset - 1) == '_')
			{
				int wordStartOffset =
					TextUtilities.GetNextCaretPosition(Document, CaretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

				string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

				InitializeCompletionWindow();
				_completionWindow.StartOffset = wordStartOffset;

				foreach (string mnemonicConstant in MnemonicData.AllConstantFlags)
					if (mnemonicConstant.StartsWith(word, StringComparison.OrdinalIgnoreCase))
						_completionWindow.CompletionList.CompletionData.Add(new CompletionData(mnemonicConstant));

				ShowCompletionWindow();
			}
		}

		private void HandleAutocompleteOnEmptyLine()
		{
			string currentSection = DocumentParser.GetSectionName(Document, CaretOffset);

			if (currentSection != null && StringHelper.BulkStringComparision(currentSection, StringComparison.OrdinalIgnoreCase,
				"Strings", "PSXStrings", "PCStrings", "ExtraNG"))
				return;

			InitializeCompletionWindow();
			_completionWindow.StartOffset = Document.GetLineByOffset(CaretOffset).Offset;

			foreach (ICompletionData item in Autocomplete.GetNewLineAutocompleteList())
				_completionWindow.CompletionList.CompletionData.Add(item);

			ShowCompletionWindow();
		}

		private void HandleAutocompleteForNextFlag()
		{
			if (!_autocompleteWorker.IsBusy)
			{
				var data = new List<object>
				{
					Text,
					CaretOffset,
					ArgumentParser.GetArgumentIndexAtOffset(Document, CaretOffset) - 1
				};

				_autocompleteWorker.RunWorkerAsync(data);
			}
		}

		private void AutocompleteWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var data = e.Argument as List<object>;

			var document = new TextDocument(data[0].ToString());
			int caretOffset = (int)data[1];
			int argumentIndex = (int)data[2];

			e.Result = Autocomplete.GetCompletionData(document, caretOffset, argumentIndex);
		}

		private void AutocompleteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var completionData = e.Result as List<ICompletionData>;

			if (completionData == null)
				return;

			if (completionData.Count == 0)
				return;

			InitializeCompletionWindow();

			int wordStartOffset =
				TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

			if (!word.StartsWith("=") && !word.StartsWith(","))
				_completionWindow.StartOffset = wordStartOffset;

			foreach (ICompletionData item in completionData)
				_completionWindow.CompletionList.CompletionData.Add(item);

			ShowCompletionWindow();
		}

		#endregion Autocomplete

		#region Error handling

		public void CheckForErrors()
		{
			if (!IsSilentSession && !_errorDetectionWorker.IsBusy)
				_errorDetectionWorker.CheckForErrorsAsync(Text);
		}

		private void ErrorWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result == null)
				return;

			ResetAllErrors();
			ApplyErrorsToLines(e.Result as List<ErrorLine>);

			TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		}

		#endregion Error handling

		#region Other public methods

		public override void TidyCode(bool trimOnly = false)
		{
			Vector scrollOffset = TextArea.TextView.ScrollOffset;

			SelectAll();
			SelectedText = trimOnly ? Cleaner.TrimEndingWhitespace(Text) : Cleaner.ReindentScript(Text);
			Select(0, 0);

			ScrollToHorizontalOffset(scrollOffset.X);
			ScrollToVerticalOffset(scrollOffset.Y);
		}

		#endregion Other public methods

		// TODO: Refactor

		private void InputFreeIndex()
		{
			string commandKey = CommandParser.GetCommandKey(Document, CaretOffset);

			if (string.IsNullOrEmpty(commandKey))
				return;

			if (commandKey.Equals("AddEffect", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("ColorRGB", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("GlobalTrigger", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("Image", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("ItemGroup", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("MultiEnvCondition", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("Organizer", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("Parameters", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("TestPosition", StringComparison.OrdinalIgnoreCase)
				|| commandKey.Equals("TriggerGroup", StringComparison.OrdinalIgnoreCase))
			{
				IEnumerable<int> takenIndicesList;

				if (DocumentParser.DocumentContainsSections(Document))
				{
					int sectionStartLineNumber = DocumentParser.GetSectionStartLine(Document, CaretOffset).LineNumber;
					takenIndicesList = GetTakenIndicesList(commandKey, sectionStartLineNumber + 1);
				}
				else
					takenIndicesList = GetTakenIndicesList(commandKey, 0);

				for (int i = 1; i < 1024; i++)
					if (!takenIndicesList.Contains(i))
					{
						TextArea.PerformTextInput(i.ToString());
						break;
					}
			}
		}

		private IEnumerable<int> GetTakenIndicesList(string commandKey, int loopStartLine)
		{
			for (int i = loopStartLine; i < Document.LineCount; i++)
			{
				DocumentLine processedLine = Document.GetLineByNumber(i);
				string processedLineText = Document.GetText(processedLine.Offset, processedLine.Length);

				string command = CommandParser.GetCommandKey(Document, processedLine.Offset);

				if (string.IsNullOrEmpty(command))
					continue;

				if (processedLineText.Contains("="))
					if (command.Equals(commandKey, StringComparison.OrdinalIgnoreCase))
					{
						int takenIndex;

						if (int.TryParse(processedLineText.Split('=')[1].Split(',')[0].Trim(), out takenIndex))
							yield return takenIndex;
					}
			}
		}

		public void UpdateSettings(CS_EditorConfiguration configuration)
		{
			SyntaxHighlighting = new SyntaxHighlighting(configuration.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(configuration.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(configuration.ColorScheme.Foreground));

			ShowSectionSeparators = configuration.ShowSectionSeparators;

			Cleaner.PreEqualSpace = configuration.Tidy_PreEqualSpace;
			Cleaner.PostEqualSpace = configuration.Tidy_PostEqualSpace;
			Cleaner.PreCommaSpace = configuration.Tidy_PreCommaSpace;
			Cleaner.PostCommaSpace = configuration.Tidy_PostCommaSpace;
			Cleaner.ReduceSpaces = configuration.Tidy_ReduceSpaces;

			base.UpdateSettings(configuration);
		}

		private void HandleDefinitionToolTips(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			DocumentLine hoveredLine = Document.GetLineByOffset(hoveredOffset);

			if (hoveredLine.HasError)
				return;

			string hoveredWord = WordParser.GetWordFromOffset(Document, hoveredOffset);
			WordType type = WordParser.GetWordTypeFromOffset(Document, hoveredOffset);

			if (type != WordType.Unknown)
			{
				ShowToolTip($"For more information about the {hoveredWord} {type}, Press F12");
				HoveredWordArgs = new WordDefinitionEventArgs(hoveredWord, type);
			}
		}

		private WordDefinitionEventArgs HoveredWordArgs = null;

		public delegate void WordDefinitionRequestedEventHandler(object sender, WordDefinitionEventArgs e);

		public event WordDefinitionRequestedEventHandler WordDefinitionRequested;
		public void OnWordDefinitionRequested(WordDefinitionEventArgs e) => WordDefinitionRequested?.Invoke(this, e);

		[Obsolete("This method shouldn't be used for ClassicScript.\nUse WordParser.GetWordFromOffset() instead.")]
		public new void GetWordFromOffset(int offset)
			=> base.GetWordFromOffset(offset);

		public override void GoTo(string objectName, object identifyingObject = null)
		{
			if (identifyingObject is ObjectType type)
			{
				DocumentLine objectLine = DocumentParser.FindDocumentLineOfObject(Document, objectName, type);

				if (objectLine != null)
				{
					Focus();
					ScrollToLine(objectLine.LineNumber);
					SelectLine(objectLine);
				}
			}
		}
	}
}
