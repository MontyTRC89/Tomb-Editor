using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Lua
{
	public sealed class LuaEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".lua";

		public bool SuppressAutocomplete { get; set; }

		#region Constructors

		public LuaEditor(Version engineVersion) : base(engineVersion)
		{
			CommentPrefix = "--";

			BindEventMethods();
		}

		private void BindEventMethods()
		{
			TextArea.TextEntering += TextArea_TextEntering;
			TextArea.TextEntered += TextEditor_TextEntered;
			TextChanged += TextEditor_TextChanged;

			KeyDown += TextEditor_KeyDown;
			MouseHover += TextEditor_MouseHover;
		}

		public override void UpdateSettings(Bases.ConfigurationBase configuration)
		{
			var config = configuration as LuaEditorConfiguration;

			string xmlFile = Path.Combine(DefaultPaths.LuaColorConfigsDirectory, "Default.xml");

			using (var stream = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
			using (var reader = new XmlTextReader(stream))
				SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));

			base.UpdateSettings(configuration);
		}

		#endregion Constructors

		#region Events

		private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && !SuppressAutocomplete
				&& e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
			{
				HandleAutocompleteAfterSpaceCtrl();
				e.Handled = true;
			}
		}

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled && !SuppressAutocomplete)
				HandleAutocomplete(e);
		}

		private void TextEditor_TextChanged(object sender, EventArgs e)
		{
			//if (LiveErrorUnderlining)
			//	_errorDetectionWorker.RunErrorCheckOnIdle(Text);
		}

		private void TextEditor_KeyDown(object sender, KeyEventArgs e)
		{
			//if (e.Key == Key.F12)
			//{
			//	if (_specialToolTip.IsOpen && HoveredWordArgs != null)
			//	{
			//		OnWordDefinitionRequested(HoveredWordArgs);
			//		HoveredWordArgs = null;
			//	}
			//	else
			//	{
			//		string word = WordParser.GetWordFromOffset(Document, CaretOffset);
			//		WordType type = WordParser.GetWordTypeFromOffset(Document, CaretOffset);

			//		if (type == WordType.Unknown && int.TryParse(word, out _))
			//			type = WordType.Decimal;

			//		if (word != null && word.StartsWith("#"))
			//		{
			//			type = WordType.Directive;
			//			word = word.Split(' ')[0];
			//		}

			//		if (!string.IsNullOrEmpty(word) && type != WordType.Unknown)
			//			OnWordDefinitionRequested(new WordDefinitionEventArgs(word, type));
			//	}
			//}
		}

		private void TextEditor_MouseHover(object sender, MouseEventArgs e)
		{
			// HandleDefinitionToolTips(e);
		}

		private void HandleAutocomplete(TextCompositionEventArgs e)
		{
			var parser = new LuaParser();
			parser.ParseLuaContent(Content);

			string currentScope = parser.GetScopeFromOffset(CaretOffset);
			IReadOnlyList<LuaElement> autocompleteItems = parser.GetAutocompleteItems(currentScope);

			if (autocompleteItems.Count == 0)
				return;

			string word = string.Empty;

			int wordStartOffset =
				TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			if (wordStartOffset != -1)
				word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

			InitializeCompletionWindow();
			_completionWindow.StartOffset = wordStartOffset;

			foreach (LuaElement item in autocompleteItems)
			{
				if (item.Name.StartsWith(word))
					_completionWindow.CompletionList.CompletionData.Add(new CompletionData(item.Name));
			}

			ShowCompletionWindow();
		}

		private void HandleAutocompleteAfterSpaceCtrl()
		{
			if (_completionWindow != null)
				return;

			DocumentLine currentLine = Document.GetLineByOffset(CaretOffset);

			int wordStartOffset =
				TextUtilities.GetNextCaretPosition(Document, CaretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);

			if (wordStartOffset == -1)
				return;

			string word = Document.GetText(wordStartOffset, CaretOffset - wordStartOffset);

			if (string.IsNullOrWhiteSpace(word))
				return;

			if (word.StartsWith("."))
			{
				int wordStartOffsetBeforeDot =
					TextUtilities.GetNextCaretPosition(Document, wordStartOffset - 1, LogicalDirection.Backward, CaretPositioningMode.WordStart);

				word = Document.GetText(wordStartOffsetBeforeDot, wordStartOffset - wordStartOffsetBeforeDot);
			}

			Console.WriteLine(word);
		}

		#endregion Events
	}
}
