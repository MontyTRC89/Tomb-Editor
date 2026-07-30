namespace TombLib.Scripting.Lua;

/// <summary>
/// Defines the Lua-specific language-service contract used by the Lua editor to provide IntelliSense features.
/// </summary>
/// <remarks>
/// This interface extends <see cref="ILanguageServerIntellisenseProvider"/> with Lua-specific concerns such as
/// semantic tokens. The generic document lifecycle, diagnostics, completion, hover, definition, and signature-help
/// contracts are defined on the base interface.
///
/// Implementations may raise callbacks from background threads. Consumers that access UI controls must marshal those
/// callbacks to the UI thread. Once disposal begins, no further provider callbacks are raised.
/// </remarks>
public interface ILuaIntellisenseProvider : ILanguageServerIntellisenseProvider
{
	/// <summary>
	/// Occurs when semantic tokens for a document have changed.
	/// </summary>
	/// <remarks>
	/// This callback may be raised from a background thread. UI consumers must marshal to the UI thread before touching
	/// controls. Once disposal begins, this event will not be raised again.
	/// </remarks>
	event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated;

	/// <summary>
	/// Gets the latest semantic tokens known for a document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <returns>The semantic tokens currently cached for the document.</returns>
	IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath);
}
