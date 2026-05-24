#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Navigation;
using TombLib.LanguageServer.Core;
using TombLib.LanguageServer.Lua;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Navigation;
using LuaLanguageServerLocator = TombIDE.ScriptingStudio.Lua.LuaLanguageServerLocator;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private readonly LuaDocumentLifecycleCoordinator _documentLifecycleCoordinator;
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly LuaIntellisenseEventBridge _intellisenseEventBridge;
	private readonly ILuaIntellisenseProvider _intellisenseProvider;
	private readonly LuaTrackedDocumentStateService _trackedDocumentStateService;

	private void HookLuaIntellisense()
	{
		_documentLifecycleCoordinator.Attach();
		_intellisenseEventBridge.Attach();

		UpdateDocumentCommandStates();
	}

	private void DisposeLuaIntellisense()
	{
		CancelPendingReferenceRequest();

		_documentLifecycleCoordinator.Dispose();
		_intellisenseEventBridge.Dispose();
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

	private void LuaEditorOpened()
	{
		RefreshLuaDiagnosticsView();
		UpdateDocumentCommandStates();
	}

	private void CurrentLuaEditorRenamed()
		=> RefreshLuaDiagnosticsView();

	private void IntellisenseProvider_DiagnosticsUpdated(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics)
	{
		_trackedDocumentStateService.ApplyDiagnosticsUpdate(filePath, diagnostics);

		if (CurrentEditor is LuaEditor currentEditor
			&& string.Equals(currentEditor.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
		{
			RefreshLuaDiagnosticsView(diagnostics: diagnostics);
		}

		UpdateDocumentCommandStates();
	}

	private void IntellisenseProvider_SemanticTokensUpdated(string filePath, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		_trackedDocumentStateService.ApplySemanticTokensUpdate(filePath, semanticTokens);

		UpdateDocumentCommandStates();
	}

	private void IntellisenseProvider_StartupFailed(LanguageServerStartupFailure failure)
	{
		if (IsDisposed)
			return;

		MessageBox.Show(this,
			failure.Message,
			failure.IsPersistent ? "Lua IntelliSense disabled" : "Lua IntelliSense unavailable",
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
	}

	private void IntellisenseProvider_WorkspaceWatcherFailed(WorkspaceWatcherFailure failure)
	{
		if (IsDisposed)
			return;

		MessageBox.Show(this,
			failure.Message,
			"Lua workspace watching disabled",
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
	}

	private void NavigateToDefinition(TextDefinitionLocation definitionLocation)
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

	private void ApplyTrackedDocumentStateToEditors(string filePath)
		=> _trackedDocumentStateService.ApplyTrackedStateToEditors(filePath);
}
