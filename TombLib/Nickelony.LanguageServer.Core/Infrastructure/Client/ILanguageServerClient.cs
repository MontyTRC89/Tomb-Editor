namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Defines the transport and capability surface used by a host provider to talk to a language server.
/// </summary>
public interface ILanguageServerClient : IDisposable, IAsyncDisposable
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
	/// Returns <see cref="TextDocumentSyncKind.None"/> until initialization completes successfully,
	/// and again after the active transport is detached or becomes unavailable.
	/// </summary>
	TextDocumentSyncKind TextDocumentSyncKind { get; }

	/// <summary>
	/// Gets the semantic token types reported by the server capabilities.
	/// The returned list is a read-only snapshot and must not be treated as mutable storage.
	/// </summary>
	IReadOnlyList<string> SemanticTokenTypes { get; }

	/// <summary>
	/// Gets the semantic token modifiers reported by the server capabilities.
	/// The returned list is a read-only snapshot and must not be treated as mutable storage.
	/// </summary>
	IReadOnlyList<string> SemanticTokenModifiers { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>completionItem/resolve</c>.
	/// Returns <see langword="false"/> until the active transport explicitly negotiates the capability.
	/// </summary>
	bool SupportsCompletionResolve { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>textDocument/references</c>.
	/// Returns <see langword="false"/> until the active transport explicitly negotiates the capability.
	/// </summary>
	bool SupportsReferences { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>textDocument/rename</c>.
	/// Returns <see langword="false"/> until the active transport explicitly negotiates the capability.
	/// </summary>
	bool SupportsRename { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports <c>textDocument/formatting</c>.
	/// Returns <see langword="false"/> until the active transport explicitly negotiates the capability.
	/// </summary>
	bool SupportsFormatting { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports full semantic token requests.
	/// Returns <see langword="false"/> until the active transport explicitly negotiates the capability.
	/// </summary>
	bool SupportsSemanticTokensFull { get; }

	/// <summary>
	/// Gets a value indicating whether the server supports semantic token delta responses.
	/// Returns <see langword="false"/> until the active transport explicitly negotiates the capability.
	/// </summary>
	bool SupportsSemanticTokensDelta { get; }

	/// <summary>
	/// Occurs when the server publishes diagnostics for a tracked document.
	/// Each subscribed handler is queued independently on the thread pool. Reentrant notifications for the same handler
	/// are serialized, and repeated pending diagnostics for the same document may coalesce to the latest payload while a handler is still busy.
	/// Different handlers may run concurrently and must marshal to a UI thread when required.
	/// </summary>
	event Action<PublishDiagnosticsParams>? DiagnosticsPublished;

	/// <summary>
	/// Occurs when the server requests a semantic token refresh for open documents.
	/// Each subscribed handler is queued independently on the thread pool. Reentrant notifications for the same handler
	/// are serialized, and repeated pending refresh requests may coalesce while a handler is still busy.
	/// Different handlers may run concurrently and must marshal to a UI thread when required.
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
	/// Marks one specific transport generation unhealthy only when it is still the active generation.
	/// </summary>
	/// <param name="transportGeneration">The observed transport generation to invalidate.</param>
	/// <returns><see langword="true"/> when the observed generation was still active and was marked unhealthy; otherwise, <see langword="false"/>.</returns>
	bool TryMarkTransportUnhealthy(long transportGeneration);

	/// <summary>
	/// Sends a JSON-RPC notification to the language server.
	/// </summary>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The notification payload.</param>
	/// <param name="cancellationToken">A token that can cancel the local dispatch attempt while the JSON-RPC notification task is still incomplete.</param>
	Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken);

	/// <summary>
	/// Sends a JSON-RPC request to the language server and waits for a typed response payload.
	/// The transport itself does not apply a default request timeout; callers own timeout and retry policy
	/// through the supplied <paramref name="cancellationToken"/>.
	/// </summary>
	/// <typeparam name="TResult">The typed response payload to deserialize.</typeparam>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The request payload.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The typed response payload.</returns>
	Task<TResult> SendRequestAsync<TResult>(string method, object parameters, CancellationToken cancellationToken);
}
