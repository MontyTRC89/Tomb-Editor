using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Lua.Services;

/// <summary>
/// Defines the language-service contract used by <see cref="LuaEditor"/> to provide Lua IntelliSense features.
/// </summary>
/// <remarks>
/// Implementations may raise callbacks from background threads. Consumers that access UI controls must marshal those
/// callbacks to the UI thread. Once disposal begins, no further provider callbacks are raised.
/// </remarks>
public interface ILuaIntellisenseProvider : IDisposable
{
	/// <summary>
	/// Gets a value indicating whether the provider is ready to serve IntelliSense requests.
	/// </summary>
	bool IsAvailable { get; }

	/// <summary>
	/// Gets a value indicating whether the provider supports Lua symbol reference requests.
	/// </summary>
	bool SupportsReferences { get; }

	/// <summary>
	/// Gets a value indicating whether the provider supports Lua symbol rename requests.
	/// </summary>
	bool SupportsRename { get; }

	/// <summary>
	/// Gets a value indicating whether the provider supports Lua document formatting requests.
	/// </summary>
	bool SupportsFormatting { get; }

	/// <summary>
	/// Occurs when diagnostics for a document have changed.
	/// </summary>
	/// <remarks>
	/// This callback may be raised from a background thread. UI consumers must marshal to the UI thread before touching
	/// controls. Once disposal begins, this event will not be raised again.
	/// </remarks>
	event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;

	/// <summary>
	/// Occurs when semantic tokens for a document have changed.
	/// </summary>
	/// <remarks>
	/// This callback may be raised from a background thread. UI consumers must marshal to the UI thread before touching
	/// controls. Once disposal begins, this event will not be raised again.
	/// </remarks>
	event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated;

	/// <summary>
	/// Gets the latest diagnostics known for a document.
	/// </summary>
	/// <param name="filePath">The path of the document.</param>
	/// <returns>The diagnostics currently cached for the document.</returns>
	IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath);

	/// <summary>
	/// Gets the latest semantic tokens known for a document.
	/// </summary>
	/// <param name="filePath">The path of the document.</param>
	/// <returns>The semantic tokens currently cached for the document.</returns>
	IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath);

	/// <summary>
	/// Opens a document in the provider and starts tracking its contents.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The initial document content.</param>
	void OpenDocument(string filePath, string content);

	/// <summary>
	/// Pushes updated content for a document that is already open in the provider.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The updated document content.</param>
	void UpdateDocument(string filePath, string content);

	/// <summary>
	/// Closes a tracked document and releases any provider-side state associated with it.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	void CloseDocument(string filePath);

	/// <summary>
	/// Rekeys a tracked document to a new path while preserving any provider-side state that still applies.
	/// </summary>
	/// <param name="oldFilePath">The previous document path.</param>
	/// <param name="newFilePath">The new document path.</param>
	/// <param name="content">The current document content.</param>
	void RenameDocument(string oldFilePath, string newFilePath, string content);

	/// <summary>
	/// Requests completion items for a position within a Lua document.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="triggerCharacter">The optional character that triggered completion.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The available completion items for the requested position.</returns>
	Task<IReadOnlyList<LuaCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
		int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests hover information for a position within a Lua document.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The hover information for the requested position, or <see langword="null"/> when unavailable.</returns>
	Task<LuaHoverInfo?> GetHoverAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests the definition location for a symbol at a position within a Lua document.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The resolved definition location, or <see langword="null"/> when no definition is available.</returns>
	Task<LuaDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests all known reference locations for a symbol at a position within a Lua document.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The resolved reference locations, or an empty list when none are available.</returns>
	Task<IReadOnlyList<LuaReferenceLocation>> GetReferencesAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests workspace edits to rename the symbol at a position within a Lua document.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="newName">The requested replacement symbol name.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The workspace edit returned by the language server, or <see langword="null"/> when none is available.</returns>
	Task<LuaWorkspaceEdit?> RenameSymbolAsync(string filePath, string content,
		int line, int column, string newName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests formatting edits for a Lua document.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="options">The editor formatting preferences to pass to the language server.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The text edits returned by the language server, or an empty list when none are available.</returns>
	Task<IReadOnlyList<LuaTextEdit>> FormatDocumentAsync(string filePath, string content,
		LuaFormattingOptions options, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests signature help for a function call at a position within a Lua document.
	/// </summary>
	/// <param name="filePath">The document path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The signature help information, or <see langword="null"/> when unavailable.</returns>
	Task<LuaSignatureInfo?> GetSignatureHelpAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default);
}
