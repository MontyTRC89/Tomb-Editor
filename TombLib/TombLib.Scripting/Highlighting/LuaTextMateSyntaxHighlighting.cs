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
		private static readonly Lazy<IGrammar> GrammarState = new Lazy<IGrammar>(LoadGrammarState);
		private static readonly TextMateTokenTheme DefaultTheme = CreateDefaultTheme();

		public static bool TryInstall(TextEditor editor, out LuaTextMateInstallation installation)
			=> TryInstall(editor, DefaultTheme, out installation);

		public static bool TryInstall(TextEditor editor, TextMateTokenTheme theme, out LuaTextMateInstallation installation)
		{
			installation = null;

			if (editor?.Document is null)
				return false;

			IGrammar grammar = GrammarState.Value;

			if (grammar is null)
				return false;

			var documentLines = new TextMateDocumentLineList(editor.Document);
			var model = new TMModel(documentLines);
			model.SetGrammar(grammar);
			var styleResolver = new TextMateThemeStyleResolver(theme ?? DefaultTheme);
			var transformer = new TextMateColorizingTransformer(editor.TextArea.TextView, model, styleResolver);

			editor.TextArea.TextView.LineTransformers.Add(transformer);
			installation = new LuaTextMateInstallation(editor, documentLines, model, transformer);
			return true;
		}

		private static IGrammar LoadGrammarState()
		{
			string grammarFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "Grammars", "Lua", "lua.tmLanguage.json");

			if (!File.Exists(grammarFilePath))
				return null;

			var registry = new Registry(new RegistryOptions(ThemeName.DarkPlus));
			return registry.LoadGrammarFromPathSync(grammarFilePath, 0, new Dictionary<string, int>());
		}

		private static TextMateTokenTheme CreateDefaultTheme()
		{
			return new TextMateTokenTheme
			{
				Rules = new List<TextMateTokenThemeRule>
				{
					new TextMateTokenThemeRule { Scope = "comment", Foreground = "#6A9955" },
					new TextMateTokenThemeRule { Scope = "string", Foreground = "#CE9178" },
					new TextMateTokenThemeRule { Scope = "constant.numeric, constant.character.escape, constant.language", Foreground = "#B5CEA8" },
					new TextMateTokenThemeRule { Scope = "keyword, storage", Foreground = "#C586C0" },
					new TextMateTokenThemeRule { Scope = "keyword.operator", Foreground = "#D4D4D4" },
					new TextMateTokenThemeRule { Scope = "entity.name.function, support.function, support.function.library, support.function.any-method", Foreground = "#DCDCAA" },
					new TextMateTokenThemeRule { Scope = "entity.name.class, support.class, support.type, storage.type.generic", Foreground = "#4EC9B0" },
					new TextMateTokenThemeRule { Scope = "variable.parameter, entity.other.attribute", Foreground = "#9CDCFE" },
					new TextMateTokenThemeRule { Scope = "variable.language.self, entity.name.tag, string.tag, storage.type.annotation", Foreground = "#569CD6" }
				}
			};
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