using TombLib.Scripting.Completion;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Editing;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Signatures;

namespace TombLib.Scripting;

/// <summary>
/// Defines the generic language-service contract used by text editors to provide IntelliSense features.
/// </summary>
/// <remarks>
/// This interface contains the parts of the IntelliSense provider contract that are not specific to any
/// single language. Language-specific concerns such as semantic tokens are layered on top through
/// narrower interfaces like <see cref="Lua.ILuaIntellisenseProvider"/>.
///
/// Implementations may raise callbacks from background threads. Consumers that access UI controls must marshal
/// those callbacks to the UI thread. Once disposal begins, no further provider callbacks are raised.
/// </remarks>
public interface ILanguageServerIntellisenseProvider : IDisposable, ITextEditProvider, ITextReferencesProvider
{
	/// <summary>
	/// Gets a value indicating whether the provider is ready to serve IntelliSense requests.
	/// </summary>
	bool IsAvailable { get; }

	/// <summary>
	/// Occurs when diagnostics for a document have changed.
	/// </summary>
	/// <remarks>
	/// This callback may be raised from a background thread. UI consumers must marshal to the UI thread before touching
	/// controls. Once disposal begins, this event will not be raised again.
	/// </remarks>
	event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;

	/// <summary>
	/// Gets the latest diagnostics known for a document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <returns>The diagnostics currently cached for the document.</returns>
	IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath);

	/// <summary>
	/// Opens a document in the provider, starts tracking its contents, and synchronizes it with the underlying language service when available.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="content">The initial document content.</param>
	void OpenDocument(string filePath, string content);

	/// <summary>
	/// Pushes updated content for a document that is already open in the provider so the underlying language service can stay synchronized.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="content">The updated document content.</param>
	void UpdateDocument(string filePath, string content);

	/// <summary>
	/// Closes a tracked document and releases any provider-side state associated with it.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	void CloseDocument(string filePath);

	/// <summary>
	/// Rekeys a tracked document to a new path while preserving any provider-side state that still applies.
	/// </summary>
	/// <param name="oldFilePath">The previous local file path.</param>
	/// <param name="newFilePath">The new local file path.</param>
	/// <param name="content">The current document content.</param>
	void RenameDocument(string oldFilePath, string newFilePath, string content);

	/// <summary>
	/// Requests completion items for a position within a document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="triggerCharacter">The optional character that triggered completion.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The available completion items for the requested position.</returns>
	Task<IReadOnlyList<TextCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
		int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests hover information for a position within a document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The hover information for the requested position, or <see langword="null"/> when unavailable.</returns>
	Task<TextHoverInfo?> GetHoverAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests the definition location for a symbol at a position within a document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The resolved definition location, or <see langword="null"/> when no definition is available.</returns>
	Task<TextDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests signature help for a function call at a position within a document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The signature help information, or <see langword="null"/> when unavailable.</returns>
	Task<TextSignatureHelpInfo?> GetSignatureHelpAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default);
}
