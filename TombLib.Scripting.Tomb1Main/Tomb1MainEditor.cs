using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Tomb1Main.Objects;
using TombLib.Scripting.Tomb1Main.Parsers;
using TombLib.Scripting.Tomb1Main.Utils;

namespace TombLib.Scripting.Tomb1Main
{
	public sealed class Tomb1MainEditor : TextEditorBase
	{
		private bool _suppressBracketAutospacing;
		private DocumentLine _cachedLine;

		public Tomb1MainEditor()
		{
			BindEventMethods();

			CommentPrefix = "//";
		}

		private void BindEventMethods()
		{
			TextArea.TextEntering += TextArea_TextEntering;
			TextArea.TextEntered += TextEditor_TextEntered;
		}

		private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				if (_completionWindow == null)
				{
					InitializeCompletionWindow();

					if (CaretOffset - 1 > 0 && Document.GetCharAt(CaretOffset - 1) == '\"')
						_completionWindow.StartOffset = CaretOffset - 1;
					else if (CaretOffset - 1 > 0 && Document.GetCharAt(CaretOffset - 1) != ' ')
					{
						int wordStartOffset =
							TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

						string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

						if (!word.StartsWith(":"))
						{
							_completionWindow.StartOffset = wordStartOffset;

							if (wordStartOffset - 1 > 0 && Document.GetCharAt(wordStartOffset - 1) == '\"')
								_completionWindow.StartOffset--;
						}
					}

					if (Document.GetCharAt(CaretOffset) == '\"')
						_completionWindow.EndOffset = CaretOffset + 1;

					foreach (ICompletionData item in Autocomplete.GetAutocompleteData())
						_completionWindow.CompletionList.CompletionData.Add(item);

					ShowCompletionWindow();
				}

				e.Handled = true;
			}

			if (!_suppressBracketAutospacing && e.Text == "\n" && CaretOffset > 0 && CaretOffset < Document.TextLength)
			{
				char prev = Document.GetCharAt(CaretOffset - 1);
				char next = Document.GetCharAt(CaretOffset);

				if ((prev == '{' && next == '}') || (prev == '[' && next == ']'))
					_suppressBracketAutospacing = true;
			}
		}

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && _completionWindow == null)
				HandleAutocomplete();

			if (_suppressBracketAutospacing && _cachedLine == null)
			{
				_cachedLine = Document.GetLineByOffset(CaretOffset);

				TextArea.PerformTextInput("\n");
				CaretOffset = _cachedLine.EndOffset;
				TextArea.PerformTextInput("\t");

				_cachedLine = null;
				_suppressBracketAutospacing = false;
			}
		}

		private void HandleAutocomplete()
		{
			string currentLineText = LineParser.EscapeComments(Document.GetText(Document.GetLineByOffset(CaretOffset))).Trim();

			if ((currentLineText.Length == 1 && char.IsLetter(currentLineText[0])) || currentLineText.Equals("\"\""))
			{
				InitializeCompletionWindow();

				if (Document.GetCharAt(CaretOffset - 1) == '\"')
					_completionWindow.StartOffset = CaretOffset - 1;
				else if (Document.GetCharAt(CaretOffset - 1) != ' ')
				{
					int wordStartOffset =
						TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

					string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

					if (!word.StartsWith(":"))
					{
						_completionWindow.StartOffset = wordStartOffset;

						if (wordStartOffset - 1 > 0 && Document.GetCharAt(wordStartOffset - 1) == '\"')
							_completionWindow.StartOffset--;
					}
				}

				if (CaretOffset < Document.TextLength && Document.GetCharAt(CaretOffset) == '\"')
					_completionWindow.EndOffset = CaretOffset + 1;

				foreach (ICompletionData item in Autocomplete.GetAutocompleteData())
					_completionWindow.CompletionList.CompletionData.Add(item);

				ShowCompletionWindow();
			}
		}

		public override void TidyCode(bool trimOnly = false)
		{
			Vector scrollOffset = TextArea.TextView.ScrollOffset;

			SelectAll();
			SelectedText = BasicCleaner.TrimEndingWhitespace(Text);
			ResetSelection();

			ScrollToHorizontalOffset(scrollOffset.X);
			ScrollToVerticalOffset(scrollOffset.Y);
		}

		public override void UpdateSettings(Bases.ConfigurationBase configuration)
		{
			var config = configuration as T1MEditorConfiguration;

			SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			BracesClosingString = config.AutoAddCommas ? "}," : "}";
			BracketsClosingString = config.AutoAddCommas ? "]," : "]";

			base.UpdateSettings(configuration);
		}

		public override void GoToObject(string objectName, object identifyingObject = null)
		{
			DocumentLine objectLine = DocumentParser.FindDocumentLineOfLevel(Document, objectName);

			if (objectLine != null)
			{
				Focus();
				ScrollToLine(objectLine.LineNumber);
				SelectLine(objectLine);
			}
		}
	}
}
