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
public interface ILuaIntellisenseProvider : IDisposable
{
	/// <summary>
	/// Gets a value indicating whether the provider is ready to serve IntelliSense requests.
	/// </summary>
	bool IsAvailable { get; }

	/// <summary>
	/// Occurs when diagnostics for a document have changed.
	/// </summary>
	event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;

	/// <summary>
	/// Occurs when semantic tokens for a document have changed.
	/// </summary>
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
