#nullable enable

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombLib.LanguageServer.Core;
using TombLib.LanguageServer.Lua;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Lua;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaIntellisenseEventBridge : IDisposable
{
	private readonly Control _owner;
	private readonly ILuaIntellisenseProvider _intellisenseProvider;
	private readonly Action<string, IReadOnlyList<TextEditorDiagnostic>> _diagnosticsUpdated;
	private readonly Action<string, IReadOnlyList<LuaSemanticToken>> _semanticTokensUpdated;
	private readonly Action<LanguageServerStartupFailure> _startupFailed;
	private readonly Action<WorkspaceWatcherFailure> _workspaceWatcherFailed;

	public LuaIntellisenseEventBridge(
		Control owner,
		ILuaIntellisenseProvider intellisenseProvider,
		Action<string, IReadOnlyList<TextEditorDiagnostic>> diagnosticsUpdated,
		Action<string, IReadOnlyList<LuaSemanticToken>> semanticTokensUpdated,
		Action<LanguageServerStartupFailure> startupFailed,
		Action<WorkspaceWatcherFailure> workspaceWatcherFailed)
	{
		_owner = owner ?? throw new ArgumentNullException(nameof(owner));
		_intellisenseProvider = intellisenseProvider ?? throw new ArgumentNullException(nameof(intellisenseProvider));
		_diagnosticsUpdated = diagnosticsUpdated ?? throw new ArgumentNullException(nameof(diagnosticsUpdated));
		_semanticTokensUpdated = semanticTokensUpdated ?? throw new ArgumentNullException(nameof(semanticTokensUpdated));
		_startupFailed = startupFailed ?? throw new ArgumentNullException(nameof(startupFailed));
		_workspaceWatcherFailed = workspaceWatcherFailed ?? throw new ArgumentNullException(nameof(workspaceWatcherFailed));
	}

	public void Attach()
	{
		Detach();

		_intellisenseProvider.DiagnosticsUpdated += IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated += IntellisenseProvider_SemanticTokensUpdated;

		if (_intellisenseProvider is LuaLanguageServerIntellisenseProvider languageServerProvider)
		{
			languageServerProvider.StartupFailed += IntellisenseProvider_StartupFailed;
			languageServerProvider.WorkspaceWatcherFailed += IntellisenseProvider_WorkspaceWatcherFailed;
		}
	}

	public void Detach()
	{
		_intellisenseProvider.DiagnosticsUpdated -= IntellisenseProvider_DiagnosticsUpdated;
		_intellisenseProvider.SemanticTokensUpdated -= IntellisenseProvider_SemanticTokensUpdated;

		if (_intellisenseProvider is LuaLanguageServerIntellisenseProvider languageServerProvider)
		{
			languageServerProvider.StartupFailed -= IntellisenseProvider_StartupFailed;
			languageServerProvider.WorkspaceWatcherFailed -= IntellisenseProvider_WorkspaceWatcherFailed;
		}
	}

	public void Dispose()
	{
		Detach();
		_intellisenseProvider.Dispose();
	}

	private void IntellisenseProvider_DiagnosticsUpdated(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> DispatchToUi(() => _diagnosticsUpdated(filePath, diagnostics));

	private void IntellisenseProvider_SemanticTokensUpdated(string filePath, IReadOnlyList<LuaSemanticToken> semanticTokens)
		=> DispatchToUi(() => _semanticTokensUpdated(filePath, semanticTokens));

	private void IntellisenseProvider_StartupFailed(LanguageServerStartupFailure failure)
		=> DispatchToUi(() => _startupFailed(failure));

	private void IntellisenseProvider_WorkspaceWatcherFailed(WorkspaceWatcherFailure failure)
		=> DispatchToUi(() => _workspaceWatcherFailed(failure));

	private void DispatchToUi(Action action)
	{
		if (_owner.IsDisposed)
			return;

		if (_owner.InvokeRequired)
		{
			_owner.BeginInvoke(action);
			return;
		}

		action();
	}
}