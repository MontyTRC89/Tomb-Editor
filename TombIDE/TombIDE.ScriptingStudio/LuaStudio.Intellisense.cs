#nullable enable

using ICSharpCode.AvalonEdit.Document;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly ILuaIntellisenseProvider _intellisenseProvider;

	private void HookLuaIntellisense()
	{
		EditorTabControl.FileOpened -= EditorTabControl_LuaFileOpened;
		EditorTabControl.FileOpened += EditorTabControl_LuaFileOpened;

		_intellisenseProvider.DiagnosticsUpdated -= IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.DiagnosticsUpdated += IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated -= IntellisenseProvider_SemanticTokensUpdated;
		_intellisenseProvider.SemanticTokensUpdated += IntellisenseProvider_SemanticTokensUpdated;

		if (_intellisenseProvider is LuaLanguageServerIntellisenseProvider languageServerProvider)
		{
			languageServerProvider.StartupFailed -= IntellisenseProvider_StartupFailed;
			languageServerProvider.StartupFailed += IntellisenseProvider_StartupFailed;
		}
	}

	private void DisposeLuaIntellisense()
	{
		EditorTabControl.FileOpened -= EditorTabControl_LuaFileOpened;
		_intellisenseProvider.DiagnosticsUpdated -= IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated -= IntellisenseProvider_SemanticTokensUpdated;

		if (_intellisenseProvider is LuaLanguageServerIntellisenseProvider languageServerProvider)
			languageServerProvider.StartupFailed -= IntellisenseProvider_StartupFailed;

		_intellisenseProvider.Dispose();
	}

	private ILuaIntellisenseProvider CreateLuaIntellisenseProvider()
	{
		string? executablePath = LuaLanguageServerLocator.ResolveExecutablePath();

		if (string.IsNullOrWhiteSpace(executablePath))
		{
			// LuaLS is shipped with TombIDE; if the bundled binary is missing the user has no way
			// of knowing why diagnostics, completion and definition lookups silently stop working.
			// Log a warning and surface a single non-blocking notification so the failure is visible.
			Log.Warn("Bundled Lua language server was not found; Lua IntelliSense (diagnostics, completion, hover, go-to-definition) will be unavailable for this session.");

			DarkUI.Forms.DarkMessageBox.Show(this,
				"The bundled Lua language server (LuaLS) could not be located.\n\n" +
				"Lua IntelliSense - including diagnostics, completion, hover and go-to-definition - will be unavailable for this session.\n\n" +
				"Reinstall TombIDE to restore the bundled language server.",
				"Lua IntelliSense unavailable",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}

		return new LuaLanguageServerIntellisenseProvider(ScriptRootDirectoryPath, executablePath);
	}

	private void EditorTabControl_LuaFileOpened(object? sender, EventArgs e)
	{
		if (sender is not LuaEditor editor)
			return;

		editor.IntellisenseProvider = _intellisenseProvider;
		editor.DefinitionNavigationRequested -= NavigateToDefinition;
		editor.DefinitionNavigationRequested += NavigateToDefinition;
		editor.TextChangedDelayed -= LuaEditor_TextChangedDelayed;
		editor.TextChangedDelayed += LuaEditor_TextChangedDelayed;

		_intellisenseProvider.OpenDocument(editor.FilePath, editor.Text);
		ApplyDiagnosticsToEditor(editor, _intellisenseProvider.GetDiagnostics(editor.FilePath));
		ApplySemanticTokensToEditor(editor, _intellisenseProvider.GetSemanticTokens(editor.FilePath));
	}

	private void LuaEditor_TextChangedDelayed(object? sender, EventArgs e)
	{
		if (sender is LuaEditor editor)
			_intellisenseProvider.UpdateDocument(editor.FilePath, editor.Text);
	}

	private void IntellisenseProvider_DiagnosticsUpdated(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<string, IReadOnlyList<TextEditorDiagnostic>>(IntellisenseProvider_DiagnosticsUpdated), filePath, diagnostics);
			return;
		}

		foreach (TabPage tabPage in EditorTabControl.FindTabPagesOfFile(filePath))
		{
			if (EditorTabControl.GetEditorOfTab(tabPage) is LuaEditor editor)
				ApplyDiagnosticsToEditor(editor, diagnostics);
		}
	}

	private void IntellisenseProvider_SemanticTokensUpdated(string filePath, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<string, IReadOnlyList<LuaSemanticToken>>(IntellisenseProvider_SemanticTokensUpdated), filePath, semanticTokens);
			return;
		}

		foreach (TabPage tabPage in EditorTabControl.FindTabPagesOfFile(filePath))
		{
			if (EditorTabControl.GetEditorOfTab(tabPage) is LuaEditor editor)
				ApplySemanticTokensToEditor(editor, semanticTokens);
		}
	}

	private void IntellisenseProvider_StartupFailed(LuaLanguageServerStartupFailure failure)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<LuaLanguageServerStartupFailure>(IntellisenseProvider_StartupFailed), failure);
			return;
		}

		if (IsDisposed)
			return;

		DarkUI.Forms.DarkMessageBox.Show(this,
			failure.Message,
			failure.IsPersistent ? "Lua IntelliSense disabled" : "Lua IntelliSense unavailable",
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
	}

	private void NavigateToDefinition(LuaDefinitionLocation definitionLocation)
	{
		if (definitionLocation is null || string.IsNullOrWhiteSpace(definitionLocation.FilePath) || !File.Exists(definitionLocation.FilePath))
			return;

		EditorTabControl.OpenFile(definitionLocation.FilePath);

		if (CurrentEditor is not TextEditorBase editor || editor.Document.LineCount == 0)
			return;

		int lineNumber = Math.Max(1, Math.Min(definitionLocation.LineNumber, editor.Document.LineCount));
		DocumentLine documentLine = editor.Document.GetLineByNumber(lineNumber);
		int columnNumber = Math.Max(1, Math.Min(definitionLocation.ColumnNumber, documentLine.Length + 1));
		int offset = documentLine.Offset + columnNumber - 1;

		editor.Focus();
		editor.CaretOffset = offset;
		editor.Select(offset, 0);
		editor.ScrollToLine(lineNumber);
	}

	private static void ApplyDiagnosticsToEditor(LuaEditor editor, IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> editor.SetDiagnostics(editor.LiveErrorUnderlining ? diagnostics : []);

	private static void ApplySemanticTokensToEditor(LuaEditor editor, IReadOnlyList<LuaSemanticToken> semanticTokens)
		=> editor.SetSemanticTokens(semanticTokens ?? []);
}
