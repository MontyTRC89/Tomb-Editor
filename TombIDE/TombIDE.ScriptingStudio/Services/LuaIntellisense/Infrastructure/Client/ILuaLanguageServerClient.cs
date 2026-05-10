#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Defines the transport and capability surface used by the Lua IntelliSense provider to talk to LuaLS.
/// </summary>
internal interface ILuaLanguageServerClient : IDisposable
{
	/// <summary>
	/// Gets a value indicating whether the language server finished initialization and can accept requests.
	/// </summary>
	bool IsReady { get; }

	/// <summary>
	/// Gets the current transport generation for the active language-server session.
	/// </summary>
	long TransportGeneration { get; }

	/// <summary>
	/// Gets the text-document synchronization mode negotiated with the language server.
	/// </summary>
	LuaTextDocumentSyncKind TextDocumentSyncKind { get; }

	/// <summary>
	/// Gets the semantic token types reported by the server capabilities.
	/// </summary>
	IReadOnlyList<string> SemanticTokenTypes { get; }

	/// <summary>
	/// Gets the semantic token modifiers reported by the server capabilities.
	/// </summary>
	IReadOnlyList<string> SemanticTokenModifiers { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>completionItem/resolve</c>.
	/// </summary>
	bool SupportsCompletionResolve { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>textDocument/references</c>.
	/// </summary>
	bool SupportsReferences { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>textDocument/rename</c>.
	/// </summary>
	bool SupportsRename { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>textDocument/formatting</c>.
	/// </summary>
	bool SupportsFormatting { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports semantic-token delta responses.
	/// </summary>
	bool SupportsSemanticTokensDelta { get; }

	/// <summary>
	/// Occurs when the server publishes diagnostics for a tracked document.
	/// </summary>
	event Action<JsonElement>? DiagnosticsPublished;

	/// <summary>
	/// Occurs when the server requests a semantic-token refresh for open documents.
	/// </summary>
	event Action? SemanticTokensRefreshRequested;

	/// <summary>
	/// Starts the language server process and completes the LSP initialization handshake.
	/// </summary>
	/// <param name="cancellationToken">A token that can cancel startup.</param>
	/// <returns><see langword="true"/> when the client is ready; otherwise, <see langword="false"/>.</returns>
	Task<bool> StartAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Marks the current transport unhealthy so the next startup check restarts the server session.
	/// </summary>
	void MarkTransportUnhealthy();

	/// <summary>
	/// Sends a JSON-RPC notification to the language server.
	/// </summary>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The notification payload.</param>
	/// <param name="cancellationToken">A token that can cancel the send operation.</param>
	Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken);

	/// <summary>
	/// Sends a JSON-RPC request to the language server and waits for the response payload.
	/// </summary>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The request payload.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The raw JSON response payload.</returns>
	Task<JsonElement> SendRequestAsync(string method, object parameters, CancellationToken cancellationToken);
}
