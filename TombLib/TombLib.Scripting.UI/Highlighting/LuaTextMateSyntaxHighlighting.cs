using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Registry;

namespace TombLib.Scripting.UI.Highlighting;

public static class LuaTextMateSyntaxHighlighting
{
	private static readonly Lazy<IGrammar?> GrammarState = new Lazy<IGrammar?>(LoadGrammarState);
	private static readonly TextMateTokenTheme DefaultTheme = LuaBuiltInTextMateThemeDefaults.CreateDefaultTextMateTheme();

	public static bool TryInstall(TextEditor editor, [NotNullWhen(true)] out LuaTextMateInstallation? installation)
		=> TryInstall(editor, DefaultTheme, out installation);

	public static bool TryInstall(TextEditor editor, TextMateTokenTheme theme, [NotNullWhen(true)] out LuaTextMateInstallation? installation)
	{
		installation = null;

		if (editor?.Document is null)
			return false;

		IGrammar? grammar = GrammarState.Value;

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

	public static IHighlightingDefinition? LoadFallbackHighlighting()
		=> LuaFallbackHighlightingLoader.Load();

	private static IGrammar? LoadGrammarState()
	{
		string grammarFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "Grammars", "Lua", "lua.tmLanguage.json");

		if (!File.Exists(grammarFilePath))
			return null;

		var registry = new Registry(new RegistryOptions(ThemeName.DarkPlus));
		return registry.LoadGrammarFromPathSync(grammarFilePath, 0, new Dictionary<string, int>());
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
