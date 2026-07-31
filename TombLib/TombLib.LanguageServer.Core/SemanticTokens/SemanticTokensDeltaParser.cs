namespace TombLib.LanguageServer.Core;

/// <summary>
/// Parses semantic token delta responses and applies them to a cached token stream.
/// </summary>
public static class SemanticTokensDeltaParser
{
	/// <summary>
	/// Parses a semantic token response that may contain either a full token stream or incremental edits.
	/// </summary>
	/// <param name="response">The raw semantic token wire response.</param>
	/// <returns>The parsed semantic token delta response.</returns>
	public static SemanticTokensDeltaResponse Parse(SemanticTokensWireResponse? response)
	{
		if (response is not { } payload)
			return new SemanticTokensDeltaResponse(ResultId: null, Data: null, Edits: null);

		if (payload.Data is { } data)
			return new SemanticTokensDeltaResponse(payload.ResultId, data, Edits: null);

		if (payload.Edits is { } editsPayload)
		{
			var edits = new List<SemanticTokensEdit>(editsPayload.Length);

			for (int i = 0; i < editsPayload.Length; i++)
			{
				SemanticTokensEditPayload edit = editsPayload[i];

				if (edit.Start is not { } start || edit.DeleteCount is not { } deleteCount)
					return new SemanticTokensDeltaResponse(payload.ResultId, Data: null, Edits: null);

				if (start < 0 || deleteCount < 0)
					return new SemanticTokensDeltaResponse(payload.ResultId, Data: null, Edits: null);

				edits.Add(new SemanticTokensEdit(start, deleteCount, edit.Data ?? []));
			}

			return new SemanticTokensDeltaResponse(payload.ResultId, Data: null, edits);
		}

		return new SemanticTokensDeltaResponse(payload.ResultId, Data: null, Edits: null);
	}

	/// <summary>
	/// Applies a list of semantic token edits to a previously cached integer stream.
	/// Returns the new stream, or <see langword="null"/> if any edit is out of range, moves backward, or overlaps a prior edit.
	/// </summary>
	/// <param name="previousData">The previously cached semantic token integer stream.</param>
	/// <param name="edits">The edits to apply in order.</param>
	/// <returns>The updated semantic token integer stream, or <see langword="null"/> when the edits are invalid.</returns>
	public static int[]? ApplyEdits(int[] previousData, IReadOnlyList<SemanticTokensEdit> edits)
	{
		int newLength = previousData.Length;
		int minimumStart = 0;

		for (int i = 0; i < edits.Count; i++)
		{
			SemanticTokensEdit edit = edits[i];

			if (edit.Start < 0 || edit.DeleteCount < 0 || edit.Start + edit.DeleteCount > previousData.Length)
				return null;

			if (edit.Start < minimumStart)
				return null;

			newLength += edit.Data.Length - edit.DeleteCount;
			minimumStart = edit.Start + edit.DeleteCount;
		}

		if (newLength < 0)
			return null;

		int[] result = new int[newLength];

		int sourceIndex = 0;
		int destinationIndex = 0;

		for (int i = 0; i < edits.Count; i++)
		{
			SemanticTokensEdit edit = edits[i];
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
