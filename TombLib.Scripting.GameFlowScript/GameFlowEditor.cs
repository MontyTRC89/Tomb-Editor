using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;
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
				if(e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
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

					if (currentLineText.Length == 1)
					{
						InitializeCompletionWindow();
						_completionWindow.StartOffset = CaretOffset - 1;

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
