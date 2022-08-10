using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Bases;
using TombLib.Scripting.GameFlowScript.Enums;
using TombLib.Scripting.GameFlowScript.Objects;
using TombLib.Scripting.GameFlowScript.Parsers;
using TombLib.Scripting.GameFlowScript.Utils;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.GameFlowScript
{
	public sealed class GameFlowEditor : TextEditorBase
	{
		public GameFlowEditor()
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

					int wordStartOffset =
						TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

					string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

					if (!word.StartsWith(":"))
						_completionWindow.StartOffset = wordStartOffset;

					foreach (ICompletionData item in Autocomplete.GetAutocompleteData())
						_completionWindow.CompletionList.CompletionData.Add(item);

					ShowCompletionWindow();
				}

				e.Handled = true;
			}
		}

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && _completionWindow == null)
				HandleAutocomplete();
		}

		private void HandleAutocomplete()
		{
			string currentLineText = LineParser.EscapeComments(Document.GetText(Document.GetLineByOffset(CaretOffset))).Trim();

			if (currentLineText.Length == 1)
			{
				InitializeCompletionWindow();

				int wordStartOffset =
					TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

				string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

				if (!word.StartsWith(":"))
					_completionWindow.StartOffset = wordStartOffset;

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
			var config = configuration as GameFlowEditorConfiguration;

			SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			base.UpdateSettings(configuration);
		}

		public override void GoToObject(string objectName, object identifyingObject = null)
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
