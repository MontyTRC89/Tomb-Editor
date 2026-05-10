#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
using TombLib.Scripting.Utils;
using TombLib.Scripting.Workers;

namespace TombLib.Scripting.ClassicScript
{
	public sealed class ClassicScriptEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".txt";

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

		public bool SuppressAutocomplete { get; set; }

		#endregion Properties

		#region Fields

		private BackgroundWorker _autocompleteWorker;
		private ErrorDetectionWorker _errorDetectionWorker;

		private IBackgroundRenderer _sectionRenderer;

		private readonly record struct AutocompleteRequest(string Text, int CaretOffset, int ArgumentIndex);

		#endregion Fields

		#region Construction

		public ClassicScriptEditor(Version engineVersion) : base(engineVersion)
		{
			InitializeBackgroundWorkers();
			InitializeRenderers();

			BindEventMethods();

			CommentPrefix = ";";
		}

		[MemberNotNull(nameof(_autocompleteWorker), nameof(_errorDetectionWorker))]
		private void InitializeBackgroundWorkers()
		{
			_autocompleteWorker = new BackgroundWorker();
			_autocompleteWorker.DoWork += AutocompleteWorker_DoWork;
			_autocompleteWorker.RunWorkerCompleted += AutocompleteWorker_RunWorkerCompleted;

			_errorDetectionWorker = new ErrorDetectionWorker(new ErrorDetector(), new Version(1, 3, 0, 7), new TimeSpan(500));
			_errorDetectionWorker.RunWorkerCompleted += ErrorWorker_RunWorkerCompleted;
		}

		[MemberNotNull(nameof(_sectionRenderer))]
		private void InitializeRenderers()
		{
			_sectionRenderer = new SectionRenderer(this);

			if (ShowSectionSeparators)
				TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
		}

		private void BindEventMethods()
		{
			TextArea.TextEntering += TextArea_TextEntering;
			TextArea.TextEntered += TextEditor_TextEntered;
			TextChanged += TextEditor_TextChanged;

			KeyDown += TextEditor_KeyDown;
			MouseHover += TextEditor_MouseHover;
		}

		#endregion Construction

		#region Events

		private void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
		{
			if (!SuppressAutocomplete)
				TryHandleCtrlSpaceCompletion(e, HandleAutocompleteAfterSpaceCtrl);
		}

		private void TextEditor_TextEntered(object? sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && !SuppressAutocomplete)
				HandleAutocomplete(e);
		}

		private void TextEditor_TextChanged(object? sender, EventArgs e)
		{
			if (LiveErrorUnderlining)
				_errorDetectionWorker.RunErrorCheckOnIdle(Text);
		}

		private void TextEditor_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.Key == Key.F12)
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

					if (type == WordType.Unknown && int.TryParse(word, out _))
						type = WordType.Decimal;

					if (word != null && word.StartsWith("#"))
					{
						type = WordType.Directive;
						word = word.Split(' ')[0];
					}

					if (!string.IsNullOrEmpty(word) && type != WordType.Unknown)
						OnWordDefinitionRequested(new WordDefinitionEventArgs(word, type));
				}
			}
		}

		private void TextEditor_MouseHover(object? sender, MouseEventArgs e)
			=> HandleDefinitionToolTips(e);

		#endregion Events

		#region Autocomplete

		// TODO: Recheck

		private void HandleAutocomplete(TextCompositionEventArgs e)
		{
			if (_completionWindow == null) // Prevents window duplicates
			{
				if (EditorCompletionTriggerHelper.IsSingleCharacterLine(Document.GetText(Document.GetLineByOffset(CaretOffset))))
					HandleAutocompleteOnEmptyLine();
				else if (e.Text != "_" && CaretOffset > 1)
					HandleAutocompleteAfterSpace();
				else if (e.Text == "_" && CaretOffset > 1)
					HandleAutocompleteOnWordWithoutContext();
				else if (e.Text == "\"" && CaretOffset > 1)
					TryHandleIncludeAutocomplete();
			}
		}

		private void HandleAutocompleteAfterSpaceCtrl()
		{
			string wholeLineText = CommandParser.GetWholeCommandLineText(Document, CaretOffset);

			if (_completionWindow == null)
			{
				if (string.IsNullOrEmpty(wholeLineText))
					HandleAutocompleteOnEmptyLine();
				else if (!_autocompleteWorker.IsBusy)
				{
					_autocompleteWorker.RunWorkerAsync(new AutocompleteRequest(Text, CaretOffset, -1));
				}
			}

			if (_completionWindow == null)
				TryHandleIncludeAutocomplete();

			if (_completionWindow == null)
				HandleAutocompleteOnWordWithoutContext();
		}

		private void HandleAutocompleteAfterSpace()
		{
			if ((Document.GetCharAt(CaretOffset - 2) == '='
				|| Document.GetCharAt(CaretOffset - 2) == ','
				|| Document.GetCharAt(CaretOffset - 2) == '_'
				|| Document.GetCharAt(CaretOffset - 2) == '+'
				|| Document.GetCharAt(CaretOffset - 2) == '-'
				|| Document.GetCharAt(CaretOffset - 2) == '*'
				|| Document.GetCharAt(CaretOffset - 2) == '/')
				&& !_autocompleteWorker.IsBusy)
			{
				_autocompleteWorker.RunWorkerAsync(new AutocompleteRequest(Text, CaretOffset, -1));
			}
			else
				TryHandleIncludeAutocomplete();
		}

		private void TryHandleIncludeAutocomplete()
		{
			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);
			string lineText = Document.GetText(currentLine);

			if (Regex.IsMatch(lineText, Patterns.IncludeCommand, RegexOptions.IgnoreCase))
			{
				int? startOffset = null;
				int? endOffset = null;

				if (Document.GetCharAt(CaretOffset - 1) == '\"')
					startOffset = CaretOffset - 1;
				else if (Document.GetCharAt(CaretOffset - 1) != ' ')
				{
					int wordStartOffset =
						TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

					string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

					if (!word.StartsWith("#"))
					{
						startOffset = wordStartOffset;

						if (wordStartOffset - 1 > 0 && Document.GetCharAt(wordStartOffset - 1) == '\"')
							startOffset--;
					}
				}

				if (CaretOffset < Document.TextLength && Document.GetCharAt(CaretOffset) == '\"')
					endOffset = CaretOffset + 1;

				string? directoryPath = Path.GetDirectoryName(FilePath);

				if (string.IsNullOrWhiteSpace(directoryPath))
					return;

				var fileDirectory = new DirectoryInfo(directoryPath);
				var completionItems = new List<ICompletionData>();

				foreach (FileInfo file in fileDirectory.GetFiles("*.txt", SearchOption.AllDirectories).Where(x => !x.FullName.Equals(FilePath)))
				{
					string pathPart = file.FullName.Replace(directoryPath, string.Empty).TrimStart('\\');
					string completionDataString = $"\"{pathPart}\"";

					completionItems.Add(new CompletionData(completionDataString));
				}

				TryOpenCompletionWindow(completionItems, startOffset, endOffset);
			}
		}

		private void HandleAutocompleteOnWordWithoutContext()
		{
			int wordStartOffset =
				TextUtilities.GetNextCaretPosition(Document, CaretOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

			if (!MnemonicData.AllConstantFlags.Any(x => x.StartsWith(word, StringComparison.OrdinalIgnoreCase)))
				return;

			var completionItems = new List<ICompletionData>();

			foreach (string mnemonicConstant in MnemonicData.AllConstantFlags)
				if (mnemonicConstant.StartsWith(word, StringComparison.OrdinalIgnoreCase))
					completionItems.Add(new CompletionData(mnemonicConstant));

			TryOpenCompletionWindow(completionItems, wordStartOffset);
		}

		private void HandleAutocompleteOnEmptyLine()
		{
			string currentSection = DocumentParser.GetCurrentSectionName(Document, CaretOffset);

			if (currentSection != null && StringHelper.BulkStringComparision(currentSection, StringComparison.OrdinalIgnoreCase,
				"Strings", "PSXStrings", "PCStrings", "ExtraNG"))
				return;

			TryOpenCompletionWindow(Autocomplete.GetNewLineAutocompleteList(), Document.GetLineByOffset(CaretOffset).Offset);
		}

		private void AutocompleteWorker_DoWork(object? sender, DoWorkEventArgs e)
		{
			if (e.Argument is not AutocompleteRequest request)
			{
				e.Result = new List<ICompletionData>();
				return;
			}

			var document = new TextDocument(request.Text);

			e.Result = Autocomplete.GetCompletionData(document, request.CaretOffset, request.ArgumentIndex);
		}

		private void AutocompleteWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
		{
			var completionData = e.Result as List<ICompletionData>;

			if (completionData == null)
				return;

			if (completionData.Count == 0)
				return;

			int wordStartOffset =
				TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);
			int? startOffset = null;

			if (!word.StartsWith("=") && !word.StartsWith(",") && !word.StartsWith("+")
				&& !word.StartsWith("-") && !word.StartsWith("*") && !word.StartsWith("/"))
				startOffset = wordStartOffset;

			TryOpenCompletionWindow(completionData, startOffset);
		}

		#endregion Autocomplete

		#region Error handling

		public void CheckForErrors()
		{
			if (!IsSilentSession && !_errorDetectionWorker.IsBusy)
				_errorDetectionWorker.CheckForErrorsAsync(Text);
		}

		private void ErrorWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result is not IReadOnlyList<TextEditorDiagnostic> diagnostics)
				return;

			SetDiagnostics(diagnostics);
		}

		#endregion Error handling

		#region Other public methods

		public override void TidyCode(bool trimOnly = false)
		{
			Vector scrollOffset = TextArea.TextView.ScrollOffset;

			SelectAll();
			SelectedText = trimOnly ? BasicCleaner.TrimEndingWhitespace(Text) : Cleaner.ReindentScript(Text);
			ResetSelection();

			ScrollToHorizontalOffset(scrollOffset.X);
			ScrollToVerticalOffset(scrollOffset.Y);
		}

		#endregion Other public methods

		// TODO: Refactor

		public void InputFreeIndex()
		{
			int nextFreeIndex = GlobalParser.GetNextFreeIndex(Document, CaretOffset);

			if (nextFreeIndex == -1)
				return;

			TextArea.PerformTextInput(nextFreeIndex.ToString());
		}

		public override void UpdateSettings(Bases.ConfigurationBase configuration)
		{
			if (configuration is not ClassicScriptEditorConfiguration config)
				return;

			SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			ShowSectionSeparators = config.ShowSectionSeparators;

			Cleaner.PreEqualSpace = config.Tidy_PreEqualSpace;
			Cleaner.PostEqualSpace = config.Tidy_PostEqualSpace;
			Cleaner.PreCommaSpace = config.Tidy_PreCommaSpace;
			Cleaner.PostCommaSpace = config.Tidy_PostCommaSpace;
			Cleaner.ReduceSpaces = config.Tidy_ReduceSpaces;

			base.UpdateSettings(configuration);
		}

		private void HandleDefinitionToolTips(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			DocumentLine hoveredLine = Document.GetLineByOffset(hoveredOffset);

			if (HasDiagnosticsOnLine(hoveredLine))
				return;

			string? hoveredWord = WordParser.GetWordFromOffset(Document, hoveredOffset);
			WordType type = WordParser.GetWordTypeFromOffset(Document, hoveredOffset);

			if (type == WordType.MnemonicConstant && !MnemonicData.AllConstantFlags.Any(x => x.Equals(hoveredWord, StringComparison.OrdinalIgnoreCase)))
				type = WordType.Unknown;

			if (type == WordType.Unknown && int.TryParse(hoveredWord, out _))
				type = WordType.Decimal;

			if (hoveredWord != null && hoveredWord.StartsWith("#"))
			{
				type = WordType.Directive;
				hoveredWord = hoveredWord.Split(' ')[0];
			}

			if (string.IsNullOrEmpty(hoveredWord))
				return;

			if (type != WordType.Unknown)
			{
				if (type == WordType.MnemonicConstant || type == WordType.Hexadecimal || type == WordType.Decimal)
				{
					string? currentFlagPrefix = ArgumentParser.GetFlagPrefixOfCurrentArgument(Document, hoveredOffset);

					if (currentFlagPrefix == null)
					{
						if (type == WordType.MnemonicConstant)
							ShowToolTip($"For more information about the \"{hoveredWord}\" Constant, Press F12.");
						else
							type = WordType.Unknown;
					}
					else
					{
						DataTable dataTable = MnemonicData.MnemonicConstantsDataTable;
						DataRow? row = null;

						switch (type)
						{
							case WordType.MnemonicConstant:
								row = dataTable.Rows.Cast<DataRow>().FirstOrDefault(r
									=> string.Equals(r[2]?.ToString(), hoveredWord, StringComparison.OrdinalIgnoreCase));
								break;

							case WordType.Hexadecimal:
								row = dataTable.Rows.Cast<DataRow>().FirstOrDefault(r
									=> string.Equals(r[1]?.ToString(), hoveredWord, StringComparison.OrdinalIgnoreCase)
									&& (r[2]?.ToString()?.StartsWith(currentFlagPrefix, StringComparison.OrdinalIgnoreCase) ?? false));
								break;

							case WordType.Decimal:
								row = dataTable.Rows.Cast<DataRow>().FirstOrDefault(r
									=> string.Equals(r[0]?.ToString(), hoveredWord, StringComparison.OrdinalIgnoreCase)
									&& (r[2]?.ToString()?.StartsWith(currentFlagPrefix, StringComparison.OrdinalIgnoreCase) ?? false));
								break;
						}

						if (row == null)
							type = WordType.Unknown;
						else
						{
							ShowToolTip($"{row[2]}\n" +
								$"{row[1]}\n" +
								$"{row[0]}\n\n" +
								$"For more information about the \"{row[2]}\" Constant, Press F12.");
						}
					}
				}
				else
					ShowToolTip($"For more information about the \"{hoveredWord}\" {type}, Press F12.");

				HoveredWordArgs = new WordDefinitionEventArgs(hoveredWord, type, hoveredOffset);
			}
		}

		private WordDefinitionEventArgs? HoveredWordArgs;

		public delegate void WordDefinitionRequestedEventHandler(object sender, WordDefinitionEventArgs e);

		public event WordDefinitionRequestedEventHandler? WordDefinitionRequested;
		public void OnWordDefinitionRequested(WordDefinitionEventArgs e) => WordDefinitionRequested?.Invoke(this, e);

		[Obsolete("This method shouldn't be used for ClassicScript.\nUse WordParser.GetWordFromOffset() instead.")]
		public new void GetWordFromOffset(int offset)
			=> base.GetWordFromOffset(offset);

		public override void GoToObject(string objectName, object? identifyingObject = null)
		{
			if (identifyingObject is ObjectType type)
			{
				DocumentLine? objectLine = DocumentParser.FindDocumentLineOfObject(Document, objectName, type);

				if (objectLine != null)
				{
					Focus();
					ScrollToLine(objectLine.LineNumber);
					SelectLine(objectLine);
				}
			}
		}

		public bool TryAddNewPluginEntry(string pluginString)
		{
			DocumentLine optionsSectionLine = DocumentParser.FindDocumentLineOfSection(Document, "Options");

			if (optionsSectionLine == null)
				return false;

			if (DocumentParser.IsPluginDefined(Document, pluginString))
				return false;

			int nextFreePluginIndex = GlobalParser.GetNextFreeIndex(Document, optionsSectionLine.Offset, "Plugin");
			DocumentLine lastSectionLine = DocumentParser.GetLastLineOfCurrentSection(Document, optionsSectionLine.Offset);

			CaretOffset = lastSectionLine.Offset + lastSectionLine.Length;

			TextArea.PerformTextInput($"{Environment.NewLine}Plugin= {nextFreePluginIndex}, {pluginString}, IGNORE");

			return true;
		}
	}
}
