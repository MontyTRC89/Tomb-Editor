#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
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
using TombLib.Scripting.Utils;

namespace TombLib.Scripting.GameFlowScript
{
	public sealed class GameFlowEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".txt";

		public GameFlowEditor(Version engineVersion) : base(engineVersion)
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
			TryHandleCtrlSpaceCompletion(e, TryShowAutocompleteWindow);
		}

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && _completionWindow == null)
				HandleAutocomplete();
		}

		private void HandleAutocomplete()
		{
			string currentLineText = LineParser.EscapeComments(Document.GetText(Document.GetLineByOffset(CaretOffset))).Trim();

			if (EditorCompletionTriggerHelper.IsSingleCharacterLine(currentLineText))
				TryShowAutocompleteWindow();
		}

		private void TryShowAutocompleteWindow()
		{
			int wordStartOffset =
				TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

			string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);
			int? startOffset = word.StartsWith(":") ? null : wordStartOffset;

			TryOpenCompletionWindow(Autocomplete.GetAutocompleteData(), startOffset);
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
			if (configuration is not GameFlowEditorConfiguration config)
				return;

			SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			base.UpdateSettings(configuration);
		}

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
	}
}
