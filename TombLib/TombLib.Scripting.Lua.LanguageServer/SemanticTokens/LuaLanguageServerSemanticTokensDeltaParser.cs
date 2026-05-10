using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Result of a `textDocument/semanticTokens/full/delta` response: either the server returned a full
/// `data` array (preferred when it is cheaper than a delta), or it returned a list of `edits` that
/// should be applied to the client-side cached integer stream.
/// </summary>
/// <param name="ResultId">The result id that can seed the next delta request.</param>
/// <param name="Data">The full semantic-token integer stream, when provided.</param>
/// <param name="Edits">The incremental edits, when provided instead of <paramref name="Data"/>.</param>
public readonly record struct LuaSemanticTokensDeltaResponse(
	string? ResultId,
	int[]? Data,
	IReadOnlyList<LuaSemanticTokensEdit>? Edits);

/// <summary>
/// Represents a single edit against a cached semantic-token integer stream.
/// </summary>
/// <param name="Start">The zero-based start offset within the integer stream.</param>
/// <param name="DeleteCount">The number of integers to remove.</param>
/// <param name="Data">The replacement integers to insert.</param>
public readonly record struct LuaSemanticTokensEdit(int Start, int DeleteCount, int[] Data);

/// <summary>
/// Parses LuaLS semantic-token delta responses and applies them to the cached token stream.
/// </summary>
public static class LuaLanguageServerSemanticTokensDeltaParser
{
	/// <summary>
	/// Parses a semantic-token response that may contain either a full token stream or incremental edits.
	/// </summary>
	/// <param name="response">The typed response payload.</param>
	/// <returns>The parsed semantic-token delta response.</returns>
	public static LuaSemanticTokensDeltaResponse Parse(LuaSemanticTokensWireResponse? response)
	{
		if (response is not { } payload)
			return new LuaSemanticTokensDeltaResponse(ResultId: null, Data: null, Edits: null);

		if (payload.Data is { } data)
			return new LuaSemanticTokensDeltaResponse(payload.ResultId, data, Edits: null);

		if (payload.Edits is { } editsPayload)
		{
			var edits = new List<LuaSemanticTokensEdit>(editsPayload.Length);

			for (int i = 0; i < editsPayload.Length; i++)
			{
				LuaSemanticTokensEditPayload edit = editsPayload[i];
				edits.Add(new LuaSemanticTokensEdit(edit.Start ?? 0, edit.DeleteCount ?? 0, edit.Data ?? []));
			}

			return new LuaSemanticTokensDeltaResponse(payload.ResultId, Data: null, edits);
		}

		return new LuaSemanticTokensDeltaResponse(payload.ResultId, Data: null, Edits: null);
	}

	/// <summary>
	/// Applies a list of LSP semantic-token edits (as returned by `semanticTokens/full/delta`) to a
	/// previously cached integer stream. Returns the new stream, or <see langword="null"/> if any
	/// edit is out of range.
	/// </summary>
	public static int[]? ApplyEdits(int[] previousData, IReadOnlyList<LuaSemanticTokensEdit> edits)
	{
		// LSP requires edits to be sorted by ascending start; we re-sort defensively in case the server
		// or our own caching layer reorders them. Edits are then applied left-to-right with a running
		// source/destination cursor so the resulting integer stream stays consistent regardless of
		// individual edit sizes.
		var ordered = new List<LuaSemanticTokensEdit>(edits);
		ordered.Sort(static (a, b) => a.Start.CompareTo(b.Start));

		int newLength = previousData.Length;

		foreach (LuaSemanticTokensEdit edit in ordered)
		{
			if (edit.Start < 0 || edit.DeleteCount < 0 || edit.Start + edit.DeleteCount > previousData.Length)
				return null;

			newLength += edit.Data.Length - edit.DeleteCount;
		}

		if (newLength < 0)
			return null;

		int[] result = new int[newLength];
		int sourceIndex = 0;
		int destinationIndex = 0;

		foreach (LuaSemanticTokensEdit edit in ordered)
		{
			int copyLength = edit.Start - sourceIndex;

			if (copyLength > 0)
			{
				Array.Copy(previousData, sourceIndex, result, destinationIndex, copyLength);
				destinationIndex += copyLength;
			}

			if (edit.Data.Length > 0)
			{
				Array.Copy(edit.Data, 0, result, destinationIndex, edit.Data.Length);
				destinationIndex += edit.Data.Length;
			}

			sourceIndex = edit.Start + edit.DeleteCount;
		}

		int tailLength = previousData.Length - sourceIndex;

		if (tailLength > 0)
		{
			Array.Copy(previousData, sourceIndex, result, destinationIndex, tailLength);
			destinationIndex += tailLength;
		}

		return destinationIndex == newLength ? result : null;
	}
}

/// <summary>
/// Decodes a raw LuaLS semantic-token integer stream (already cached on the client) into the typed
/// <see cref="LuaSemanticToken"/> list expected by the editor's colorizer.
/// </summary>
public static class LuaLanguageServerSemanticTokensDecoder
{
	private static readonly IReadOnlyList<string> EmptyModifiers = [];

	/// <summary>
	/// Decodes a raw semantic-token integer stream into the typed token objects expected by the editor.
	/// </summary>
	/// <param name="data">The raw LSP semantic-token integer stream.</param>
	/// <param name="document">The document snapshot associated with the token stream.</param>
	/// <param name="tokenTypes">The semantic token types advertised by the server.</param>
	/// <param name="tokenModifiers">The semantic token modifiers advertised by the server.</param>
	/// <returns>The decoded semantic tokens.</returns>
	public static IReadOnlyList<LuaSemanticToken> Decode(int[] data, LuaDocumentSnapshot? document,
		IReadOnlyList<string>? tokenTypes, IReadOnlyList<string>? tokenModifiers)
	{
		if (data.Length == 0 || document is null || tokenTypes is null || tokenTypes.Count == 0)
			return [];

		LuaDocumentLineOffsets lineOffsets = LuaDocumentLineOffsets.Build(document.Content);
		var semanticTokens = new List<LuaSemanticToken>(data.Length / 5);
		Dictionary<int, IReadOnlyList<string>>? modifierCache = null;

		int line = 0;
		int character = 0;

		for (int tupleStart = 0; tupleStart + 4 < data.Length; tupleStart += 5)
		{
			int deltaLine = data[tupleStart];
			int deltaCharacter = data[tupleStart + 1];
			int length = data[tupleStart + 2];
			int tokenTypeIndex = data[tupleStart + 3];
			int modifierMask = data[tupleStart + 4];

			line += deltaLine;
			character = deltaLine == 0 ? character + deltaCharacter : deltaCharacter;

			if (line < 0 || line >= lineOffsets.LineCount || tokenTypeIndex < 0 || tokenTypeIndex >= tokenTypes.Count)
				continue;

			int lineLength = lineOffsets.GetLineLength(line);
			int safeCharacter = Math.Max(0, Math.Min(character, lineLength));
			int safeLength = Math.Max(0, Math.Min(length, lineLength - safeCharacter));

			if (safeLength == 0)
				continue;

			semanticTokens.Add(new LuaSemanticToken(
				line,
				safeCharacter,
				safeLength,
				tokenTypes[tokenTypeIndex],
				GetOrAddModifiers(ref modifierCache, modifierMask, tokenModifiers)));
		}

		return semanticTokens;
	}

	private static IReadOnlyList<string> GetOrAddModifiers(
		ref Dictionary<int, IReadOnlyList<string>>? cache,
		int modifierMask,
		IReadOnlyList<string>? tokenModifiers)
	{
		if (modifierMask == 0 || tokenModifiers is null || tokenModifiers.Count == 0)
			return EmptyModifiers;

		cache ??= [];

		if (cache.TryGetValue(modifierMask, out IReadOnlyList<string>? cached))
			return cached;

		var modifiers = new List<string>();

		for (int bitIndex = 0; bitIndex < tokenModifiers.Count; bitIndex++)
		{
			if ((modifierMask & (1 << bitIndex)) != 0)
				modifiers.Add(tokenModifiers[bitIndex]);
		}

		cache[modifierMask] = modifiers;
		return modifiers;
	}
}
