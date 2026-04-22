#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal interface ILuaLanguageServerClient : IDisposable
{
	bool IsReady { get; }
	LuaTextDocumentSyncKind TextDocumentSyncKind { get; }
	IReadOnlyList<string> SemanticTokenTypes { get; }
	IReadOnlyList<string> SemanticTokenModifiers { get; }
	bool SupportsCompletionResolve { get; }
	bool SupportsSemanticTokensDelta { get; }

	event Action<JsonElement>? DiagnosticsPublished;
	event Action? SemanticTokensRefreshRequested;

	Task<bool> StartAsync(CancellationToken cancellationToken);
	Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken);
	Task<JsonElement> SendRequestAsync(string method, object parameters, CancellationToken cancellationToken);
}
