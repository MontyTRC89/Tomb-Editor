namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Caches the latest semantic tokens and delta state published for a tracked Lua document.
/// </summary>
internal sealed class LuaSemanticTokensCache
{
	private static readonly IReadOnlyList<LuaSemanticToken> EmptyTokens = Array.AsReadOnly(Array.Empty<LuaSemanticToken>());

	/// <summary>
	/// Gets the latest decoded semantic tokens.
	/// </summary>
	internal IReadOnlyList<LuaSemanticToken> Tokens { get; private set; } = EmptyTokens;

	/// <summary>
	/// Gets the synchronized document version associated with the cached semantic tokens.
	/// </summary>
	internal int Version { get; private set; }

	/// <summary>
	/// Gets the previously cached raw semantic token data stream used for delta requests.
	/// </summary>
	internal int[]? PreviousData { get; private set; }

	/// <summary>
	/// Gets the previously cached semantic token result identifier used for delta requests.
	/// </summary>
	internal string? PreviousResultId { get; private set; }

	/// <summary>
	/// Clears the token cache and all delta-request state.
	/// </summary>
	internal void Clear()
	{
		Tokens = EmptyTokens;
		Version = 0;
		PreviousData = null;
		PreviousResultId = null;
	}

	/// <summary>
	/// Returns the current semantic token delta state used for incremental refreshes.
	/// </summary>
	/// <returns>The current delta-request state.</returns>
	internal SemanticTokensDeltaState GetDeltaState()
		=> new(PreviousResultId, CloneData(PreviousData));

	/// <summary>
	/// Drops server-side synchronization state while preserving the last decoded token list.
	/// </summary>
	internal void InvalidateServerSynchronization()
	{
		Version = 0;
		PreviousData = null;
		PreviousResultId = null;
	}

	/// <summary>
	/// Stores the raw semantic token delta-request state returned by the language server.
	/// </summary>
	/// <param name="resultId">The server-provided result identifier.</param>
	/// <param name="data">The raw semantic token integer stream.</param>
	internal void StoreDeltaState(string? resultId, int[]? data)
	{
		PreviousResultId = resultId;
		PreviousData = CloneData(data);
	}

	/// <summary>
	/// Stores decoded semantic tokens when their version is not stale relative to the current cache.
	/// </summary>
	/// <param name="version">The synchronized document version.</param>
	/// <param name="semanticTokens">The decoded semantic tokens.</param>
	/// <returns><see langword="true"/> when the token set was stored; otherwise, <see langword="false"/>.</returns>
	internal bool TryStore(int version, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		if (!LuaDocumentVersionHelper.TryAccept(Version, version, out int acceptedVersion))
			return false;

		Version = acceptedVersion;

		Tokens = CreateReadOnlyTokens(semanticTokens);
		return true;
	}

	private static int[]? CloneData(int[]? data)
		=> data is null ? null : [.. data];

	private static IReadOnlyList<LuaSemanticToken> CreateReadOnlyTokens(IReadOnlyList<LuaSemanticToken>? semanticTokens)
	{
		return semanticTokens is null || semanticTokens.Count == 0
			? EmptyTokens
			: Array.AsReadOnly([.. semanticTokens]);
	}
}
