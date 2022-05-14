using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;
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
		public Tomb1MainEditor()
		{
			BindEventMethods();

			CommentPrefix = "//";
		}

		private void BindEventMethods()
		{
			TextArea.TextEntered += TextEditor_TextEntered;
		}

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled)
				HandleAutocomplete(e);
		}

		private void HandleAutocomplete(TextCompositionEventArgs e)
		{
			if (_completionWindow == null) // Prevents window duplicates
			{
				if (e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
				{
					Select(CaretOffset - 1, 1);
					SelectedText = string.Empty;

					InitializeCompletionWindow();
					_completionWindow.StartOffset = CaretOffset;

					foreach (ICompletionData item in Autocomplete.GetAutocompleteData())
						_completionWindow.CompletionList.CompletionData.Add(item);

					ShowCompletionWindow();
				}
				else
				{
					string currentLineText = LineParser.EscapeComments(Document.GetText(Document.GetLineByOffset(CaretOffset))).Trim();

					if (currentLineText.Length == 1 || currentLineText.Equals("\"\""))
					{
						InitializeCompletionWindow();
						_completionWindow.StartOffset = CaretOffset - 1;

						if (currentLineText.Equals("\"\""))
							_completionWindow.EndOffset = CaretOffset + 1;

						foreach (ICompletionData item in Autocomplete.GetAutocompleteData())
							_completionWindow.CompletionList.CompletionData.Add(item);

						ShowCompletionWindow();
					}
				}
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

		public override void UpdateSettings(ConfigurationBase configuration)
		{
			var config = configuration as T1MEditorConfiguration;

			SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

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
