namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Result of a <c>textDocument/semanticTokens/full/delta</c> response: either the server returned a full
/// <c>data</c> array, or it returned a list of <c>edits</c> that should be applied to the cached integer stream.
/// </summary>
/// <param name="ResultId">The result identifier associated with the returned token data.</param>
/// <param name="Data">The full semantic token integer stream, when the server returned one.</param>
/// <param name="Edits">The incremental edits to apply to the cached token stream, when provided.</param>
public readonly record struct SemanticTokensDeltaResponse(
	string? ResultId,
	int[]? Data,
	IReadOnlyList<SemanticTokensEdit>? Edits);
