#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;
using TombLib.LanguageServer.Core;
using TombLib.LanguageServer.Lua;
using TombLib.Scripting.Diagnostics;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaIntellisenseEventBridge : ILuaIntellisenseBridge
{
	private readonly IAvalonDockHost _dockHost;
	private readonly IMessenger _messenger;
	private readonly ILuaIntellisenseProvider _intellisenseProvider;

	public LuaIntellisenseEventBridge(
		IAvalonDockHost dockHost,
		IMessenger messenger,
		ILuaIntellisenseProvider intellisenseProvider)
	{
		_dockHost = dockHost ?? throw new ArgumentNullException(nameof(dockHost));
		_messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
		_intellisenseProvider = intellisenseProvider ?? throw new ArgumentNullException(nameof(intellisenseProvider));
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
		=> DispatchToUi(() => _messenger.Send(new LuaDiagnosticsUpdatedMessage(new LuaDiagnosticsPayload(filePath, diagnostics))));

	private void IntellisenseProvider_SemanticTokensUpdated(string filePath, IReadOnlyList<LuaSemanticToken> semanticTokens)
		=> DispatchToUi(() => _messenger.Send(new LuaSemanticTokensUpdatedMessage(new LuaSemanticTokensPayload(filePath, semanticTokens))));

	private void IntellisenseProvider_StartupFailed(LanguageServerStartupFailure failure)
		=> DispatchToUi(() => _messenger.Send(new LuaStartupFailedMessage(failure)));

	private void IntellisenseProvider_WorkspaceWatcherFailed(WorkspaceWatcherFailure failure)
		=> DispatchToUi(() => _messenger.Send(new LuaWorkspaceWatcherFailedMessage(failure)));

	private void DispatchToUi(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		if (_dockHost.Dispatcher.HasShutdownStarted || _dockHost.Dispatcher.HasShutdownFinished)
			return;

		if (_dockHost.Dispatcher.CheckAccess())
		{
			action();
			return;
		}

		_ = _dockHost.Dispatcher.BeginInvoke(action);
	}
}
