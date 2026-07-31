using System.Diagnostics.CodeAnalysis;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Converts LSP protocol positions and ranges into one-based editor-friendly coordinates.
/// </summary>
public static class ProtocolRangeHelper
{
	/// <summary>
	/// Converts a protocol range payload into a one-based document range.
	/// </summary>
	/// <param name="rangePayload">The protocol range payload to convert.</param>
	/// <param name="range">Receives the converted one-based document range.</param>
	/// <returns><see langword="true"/> when the range payload contained valid start and end positions.</returns>
	public static bool TryGetOneBasedRange(ProtocolRangePayload? rangePayload, [NotNullWhen(true)] out OneBasedDocumentRange? range)
	{
		range = null;

		if (!TryGetOneBasedLineAndColumn(rangePayload?.Start, out int startLineNumber, out int startColumnNumber)
			|| !TryGetOneBasedLineAndColumn(rangePayload?.End, out int endLineNumber, out int endColumnNumber))
		{
			return false;
		}

		range = new OneBasedDocumentRange(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber);
		return true;
	}

	/// <summary>
	/// Converts a protocol position into one-based line and column coordinates.
	/// </summary>
	/// <param name="position">The protocol position to convert.</param>
	/// <param name="lineNumber">Receives the one-based line number.</param>
	/// <param name="columnNumber">Receives the one-based column number.</param>
	/// <returns><see langword="true"/> when the protocol position contained both line and character values.</returns>
	public static bool TryGetOneBasedLineAndColumn(ProtocolNullablePosition? position, out int lineNumber, out int columnNumber)
	{
		lineNumber = 1;
		columnNumber = 1;

		if (position is not { Line: int parsedLine, Character: int parsedCharacter })
			return false;

		if (parsedLine < 0 || parsedCharacter < 0)
			return false;

		lineNumber = parsedLine + 1;
		columnNumber = parsedCharacter + 1;
		return true;
	}
}
