using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Registry;

namespace TombLib.Scripting.UI.Highlighting
{
	public static class LuaTextMateSyntaxHighlighting
	{
		private static readonly Lazy<IGrammar> GrammarState = new Lazy<IGrammar>(LoadGrammarState);
		private static readonly Lazy<IHighlightingDefinition> FallbackHighlightingState = new Lazy<IHighlightingDefinition>(LoadFallbackHighlightingCore);
		private static readonly TextMateTokenTheme DefaultTheme = LuaBuiltInTextMateThemeDefaults.CreateDefaultTextMateTheme();

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

		public static IHighlightingDefinition LoadFallbackHighlighting()
			=> FallbackHighlightingState.Value;

		private static IGrammar LoadGrammarState()
		{
			string grammarFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "Grammars", "Lua", "lua.tmLanguage.json");

			if (!File.Exists(grammarFilePath))
				return null;

			var registry = new Registry(new RegistryOptions(ThemeName.DarkPlus));
			return registry.LoadGrammarFromPathSync(grammarFilePath, 0, new Dictionary<string, int>());
		}

		private static IHighlightingDefinition LoadFallbackHighlightingCore()
		{
			string fallbackFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "ColorSchemes", "Lua", "Default.xml");

			if (!File.Exists(fallbackFilePath))
				return null;

			using var stream = File.OpenRead(fallbackFilePath);
			using var reader = XmlReader.Create(stream);
			return HighlightingLoader.Load(reader, HighlightingManager.Instance);
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
