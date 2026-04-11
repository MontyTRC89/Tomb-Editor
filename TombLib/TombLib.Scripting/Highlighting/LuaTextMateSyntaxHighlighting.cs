using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.AvalonEdit;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace TombLib.Scripting.Highlighting
{
	public static class LuaTextMateSyntaxHighlighting
	{
		private static readonly Lazy<LuaTextMateGrammarState> GrammarState = new Lazy<LuaTextMateGrammarState>(LoadGrammarState);

		public static bool TryInstall(TextEditor editor, out LuaTextMateInstallation installation)
		{
			installation = null;

			if (editor?.Document is null)
				return false;

			LuaTextMateGrammarState grammarState = GrammarState.Value;

			if (grammarState?.Grammar is null || grammarState.Theme is null)
				return false;

			var documentLines = new TextMateDocumentLineList(editor.Document);
			var model = new TMModel(documentLines);
			model.SetGrammar(grammarState.Grammar);
			var styleResolver = new TextMateThemeStyleResolver(grammarState.Theme);
			var transformer = new TextMateColorizingTransformer(editor.TextArea.TextView, model, styleResolver);

			editor.TextArea.TextView.LineTransformers.Add(transformer);
			installation = new LuaTextMateInstallation(editor, documentLines, model, transformer);
			return true;
		}

		private static LuaTextMateGrammarState LoadGrammarState()
		{
			string grammarFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "Grammars", "Lua", "lua.tmLanguage.json");

			if (!File.Exists(grammarFilePath))
				return null;

			var registry = new Registry(new RegistryOptions(ThemeName.DarkPlus));
			IGrammar grammar = registry.LoadGrammarFromPathSync(grammarFilePath, 0, new Dictionary<string, int>());
			Theme theme = registry.GetTheme();

			return grammar is null || theme is null
				? null
				: new LuaTextMateGrammarState(grammar, theme);
		}

		private sealed class LuaTextMateGrammarState
		{
			public LuaTextMateGrammarState(IGrammar grammar, Theme theme)
			{
				Grammar = grammar;
				Theme = theme;
			}

			public IGrammar Grammar { get; }
			public Theme Theme { get; }
		}
	}

	public sealed class LuaTextMateInstallation : IDisposable
	{
		private readonly TextEditor _editor;
		private readonly TextMateDocumentLineList _documentLines;
		private readonly TextMateColorizingTransformer _transformer;
		private bool _isDisposed;

		internal LuaTextMateInstallation(TextEditor editor, TextMateDocumentLineList documentLines, TMModel model,
			TextMateColorizingTransformer transformer)
		{
			_editor = editor;
			_documentLines = documentLines;
			_transformer = transformer;
			Model = model;
		}

		public TMModel Model { get; }

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			_transformer.Dispose();

			if (_editor.TextArea.TextView.LineTransformers.Contains(_transformer))
				_editor.TextArea.TextView.LineTransformers.Remove(_transformer);

			Model.Dispose();
			_documentLines.Dispose();
		}
	}
}