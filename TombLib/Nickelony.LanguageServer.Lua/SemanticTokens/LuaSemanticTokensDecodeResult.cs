namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Represents the decoded semantic tokens payload and whether the caller should retry with a full refresh.
/// </summary>
/// <param name="Tokens">The decoded semantic tokens ready for the editor.</param>
/// <param name="Data">The raw semantic token integer stream to cache for future delta requests.</param>
/// <param name="ResultId">The server-provided semantic token result identifier.</param>
/// <param name="RetryWithFullRefresh"><see langword="true"/> when the caller should discard delta state and request a full refresh.</param>
internal readonly record struct LuaSemanticTokensDecodeResult(
	IReadOnlyList<LuaSemanticToken> Tokens,
	int[]? Data,
	string? ResultId,
	bool RetryWithFullRefresh);
