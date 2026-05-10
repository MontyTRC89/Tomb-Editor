#nullable enable

using ICSharpCode.AvalonEdit.Document;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using LuaLanguageServerLocator = TombIDE.ScriptingStudio.Services.LuaIntellisense.LuaLanguageServerLocator;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Objects;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua.LanguageServer;
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
		EditorTabControl.SelectedIndexChanged -= EditorTabControl_LuaSelectedIndexChanged;
		EditorTabControl.SelectedIndexChanged += EditorTabControl_LuaSelectedIndexChanged;
		EditorTabControl.DocumentRenamed -= EditorTabControl_DocumentRenamed;
		EditorTabControl.DocumentRenamed += EditorTabControl_DocumentRenamed;

		_intellisenseProvider.DiagnosticsUpdated -= IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.DiagnosticsUpdated += IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated -= IntellisenseProvider_SemanticTokensUpdated;
		_intellisenseProvider.SemanticTokensUpdated += IntellisenseProvider_SemanticTokensUpdated;

		if (_intellisenseProvider is LuaLanguageServerIntellisenseProvider languageServerProvider)
		{
			languageServerProvider.StartupFailed -= IntellisenseProvider_StartupFailed;
			languageServerProvider.StartupFailed += IntellisenseProvider_StartupFailed;
			languageServerProvider.WorkspaceWatcherFailed -= IntellisenseProvider_WorkspaceWatcherFailed;
			languageServerProvider.WorkspaceWatcherFailed += IntellisenseProvider_WorkspaceWatcherFailed;
		}

		UpdateLuaFeatureCommandAvailability();
	}

	private void DisposeLuaIntellisense()
	{
		CancelPendingReferenceRequest();

		EditorTabControl.FileOpened -= EditorTabControl_LuaFileOpened;
		EditorTabControl.SelectedIndexChanged -= EditorTabControl_LuaSelectedIndexChanged;
		EditorTabControl.DocumentRenamed -= EditorTabControl_DocumentRenamed;
		_intellisenseProvider.DiagnosticsUpdated -= IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated -= IntellisenseProvider_SemanticTokensUpdated;

		if (_intellisenseProvider is LuaLanguageServerIntellisenseProvider languageServerProvider)
		{
			languageServerProvider.StartupFailed -= IntellisenseProvider_StartupFailed;
			languageServerProvider.WorkspaceWatcherFailed -= IntellisenseProvider_WorkspaceWatcherFailed;
		}

		_intellisenseProvider.Dispose();
	}

	private ILuaIntellisenseProvider CreateLuaIntellisenseProvider()
	{
		string? executablePath = LuaLanguageServerLocator.ResolveExecutablePath();

		if (string.IsNullOrWhiteSpace(executablePath))
		{
			// LuaLS is shipped with TombIDE; if the bundled binary is missing the user has no way
			// of knowing why diagnostics, completion, definition and references silently stop working.
			// Log a warning and surface a single non-blocking notification so the failure is visible.
			Log.Warn("Bundled Lua language server was not found; Lua IntelliSense (diagnostics, completion, hover, go-to-definition and find references) will be unavailable for this session.");

			MessageBox.Show(this,
				"The bundled Lua language server (LuaLS) could not be located.\n\n" +
				"Lua IntelliSense - including diagnostics, completion, hover, go-to-definition and find references - will be unavailable for this session.\n\n" +
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
		editor.StatusChanged -= LuaEditor_StatusChanged;
		editor.StatusChanged += LuaEditor_StatusChanged;
		editor.TextChanged -= LuaEditor_TextChanged;
		editor.TextChanged += LuaEditor_TextChanged;
		editor.TextChangedDelayed -= LuaEditor_TextChangedDelayed;
		editor.TextChangedDelayed += LuaEditor_TextChangedDelayed;

		_intellisenseProvider.OpenDocument(editor.FilePath, editor.Text);
		ApplyDiagnosticsToEditor(editor, _intellisenseProvider.GetDiagnostics(editor.FilePath));
		ApplySemanticTokensToEditor(editor, _intellisenseProvider.GetSemanticTokens(editor.FilePath));
		RefreshLuaDiagnosticsView();
		UpdateLuaFeatureCommandAvailability();
	}

	private void LuaEditor_TextChangedDelayed(object? sender, EventArgs e)
	{
		if (sender is LuaEditor editor)
			_intellisenseProvider.UpdateDocument(editor.FilePath, editor.Text);
	}

	private void EditorTabControl_DocumentRenamed(object? sender, DocumentRenamedEventArgs e)
	{
		LuaEditor? editor = null;

		foreach (TabPage tabPage in EditorTabControl.FindTabPagesOfFile(e.NewFilePath))
		{
			if (EditorTabControl.GetEditorOfTab(tabPage) is LuaEditor luaEditor)
			{
				editor = luaEditor;
				break;
			}
		}

		if (editor is null)
			return;

		_intellisenseProvider.RenameDocument(e.OldFilePath, e.NewFilePath, editor.Text);

		if (ReferenceEquals(CurrentEditor, editor))
			RefreshLuaDiagnosticsView();
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

		if (CurrentEditor is LuaEditor currentEditor
			&& string.Equals(currentEditor.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
		{
			RefreshLuaDiagnosticsView(diagnostics: diagnostics);
		}

		UpdateLuaFeatureCommandAvailability();
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

		UpdateLuaFeatureCommandAvailability();
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

		MessageBox.Show(this,
			failure.Message,
			failure.IsPersistent ? "Lua IntelliSense disabled" : "Lua IntelliSense unavailable",
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
	}

	private void IntellisenseProvider_WorkspaceWatcherFailed(LuaWorkspaceWatcherFailure failure)
	{
		if (InvokeRequired)
		{
			BeginInvoke(new Action<LuaWorkspaceWatcherFailure>(IntellisenseProvider_WorkspaceWatcherFailed), failure);
			return;
		}

		if (IsDisposed)
			return;

		MessageBox.Show(this,
			failure.Message,
			"Lua workspace watching disabled",
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
	}

	private void NavigateToDefinition(LuaDefinitionLocation definitionLocation)
	{
		if (definitionLocation is null || string.IsNullOrWhiteSpace(definitionLocation.FilePath) || !File.Exists(definitionLocation.FilePath))
			return;

		NavigateToLocation(
			definitionLocation.FilePath,
			NavigationOrigin.Definition,
			editor => EditorNavigationHelper.CreateDefinitionLocation(
				editor,
				definitionLocation.FilePath,
				definitionLocation.LineNumber,
				definitionLocation.ColumnNumber));
	}

	private static void ApplyDiagnosticsToEditor(LuaEditor editor, IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> editor.SetDiagnostics(editor.LiveErrorUnderlining ? diagnostics : []);

	private static void ApplySemanticTokensToEditor(LuaEditor editor, IReadOnlyList<LuaSemanticToken> semanticTokens)
		=> editor.SetSemanticTokens(semanticTokens ?? []);

	private void ApplyTrackedDocumentStateToEditors(string filePath)
	{
		IReadOnlyList<TextEditorDiagnostic> diagnostics = _intellisenseProvider.GetDiagnostics(filePath);
		IReadOnlyList<LuaSemanticToken> semanticTokens = _intellisenseProvider.GetSemanticTokens(filePath);

		foreach (TabPage tabPage in EditorTabControl.FindTabPagesOfFile(filePath))
		{
			if (EditorTabControl.GetEditorOfTab(tabPage) is LuaEditor editor)
			{
				ApplyDiagnosticsToEditor(editor, diagnostics);
				ApplySemanticTokensToEditor(editor, semanticTokens);
			}
		}
	}
}
