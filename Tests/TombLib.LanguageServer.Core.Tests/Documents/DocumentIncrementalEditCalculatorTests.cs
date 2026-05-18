using System.Text.Json;

namespace TombLib.LanguageServer.Core.Tests;

[TestClass]
public class DocumentIncrementalEditCalculatorTests
{
	[TestMethod]
	public void Compute_CollapsesUnchangedPrefixAndSuffixIntoMinimalRangeEdit()
	{
		const string oldText = "local foo = 1\nlocal bar = 2\n";
		const string newText = "local foo = 1\nlocal baz = 2\n";

		DocumentChangeRange range = DocumentIncrementalEditCalculator.Compute(oldText, newText, DocumentLineOffsets.Build(oldText));

		Assert.AreEqual(1, range.StartLine);
		Assert.AreEqual(8, range.StartCharacter);
		Assert.AreEqual(1, range.EndLine);
		Assert.AreEqual(9, range.EndCharacter);
		Assert.AreEqual("z", range.Text);
	}

	[TestMethod]
	public void Compute_HandlesPureInsertionAtEndOfFile()
	{
		const string oldText = "local foo = 1\n";
		const string newText = "local foo = 1\nlocal bar = 2\n";

		DocumentChangeRange range = DocumentIncrementalEditCalculator.Compute(oldText, newText, DocumentLineOffsets.Build(oldText));

		Assert.AreEqual(1, range.StartLine);
		Assert.AreEqual(0, range.StartCharacter);
		Assert.AreEqual(1, range.EndLine);
		Assert.AreEqual(0, range.EndCharacter);
		Assert.AreEqual("local bar = 2\n", range.Text);
	}

	[TestMethod]
	public void Compute_NoChange_ProducesEmptyRange()
	{
		const string text = "print('hi')\n";

		DocumentChangeRange range = DocumentIncrementalEditCalculator.Compute(text, text, DocumentLineOffsets.Build(text));

		Assert.AreEqual(string.Empty, range.Text);
		Assert.AreEqual(range.StartLine, range.EndLine);
		Assert.AreEqual(range.StartCharacter, range.EndCharacter);
	}
}

[TestClass]
public class DocumentLineOffsetsTests
{
	[TestMethod]
	public void Accessors_ClampOutOfRangeLineIndices()
	{
		DocumentLineOffsets lineOffsets = DocumentLineOffsets.Build("one\ntwo");

		Assert.AreEqual(3, lineOffsets.GetLineLength(-1));
		Assert.AreEqual(0, lineOffsets.GetLineStartOffset(-1));
		Assert.AreEqual(3, lineOffsets.GetLineLength(99));
		Assert.AreEqual(4, lineOffsets.GetLineStartOffset(99));
		Assert.AreEqual("two", lineOffsets.GetLineText(99));
	}
}

[TestClass]
public class DocumentRangeOffsetResolverTests
{
	[TestMethod]
	public void TryResolveOffsets_ClampsStaleLineIndicesWithoutThrowing()
	{
		DocumentLineOffsets lineOffsets = DocumentLineOffsets.Build("\nvalue");

		bool resolved = DocumentRangeOffsetResolver.TryResolveOffsets(lineOffsets,
			startLineIndex: 99,
			startCharacter: 99,
			endLineIndex: 99,
			endCharacter: 99,
			out int startOffset,
			out int endOffset);

		Assert.IsTrue(resolved);
		Assert.AreEqual(1, startOffset);
		Assert.AreEqual(6, endOffset);
	}
}

[TestClass]
public class ProtocolRangeHelperTests
{
	[TestMethod]
	public void TryGetOneBasedLineAndColumn_ReturnsFalseForNegativeProtocolCoordinates()
	{
		bool resolved = ProtocolRangeHelper.TryGetOneBasedLineAndColumn(
			new ProtocolNullablePosition(Line: -1, Character: 0),
			out int lineNumber,
			out int columnNumber);

		Assert.IsFalse(resolved);
		Assert.AreEqual(1, lineNumber);
		Assert.AreEqual(1, columnNumber);

		resolved = ProtocolRangeHelper.TryGetOneBasedLineAndColumn(
			new ProtocolNullablePosition(Line: 0, Character: -1),
			out lineNumber,
			out columnNumber);

		Assert.IsFalse(resolved);
		Assert.AreEqual(1, lineNumber);
		Assert.AreEqual(1, columnNumber);
	}

	[TestMethod]
	public void TryGetOneBasedRange_ReturnsFalseWhenRangeContainsNegativeProtocolCoordinate()
	{
		bool resolved = ProtocolRangeHelper.TryGetOneBasedRange(
			new ProtocolRangePayload(
				new ProtocolNullablePosition(Line: -1, Character: 0),
				new ProtocolNullablePosition(Line: 0, Character: 1)),
			out OneBasedDocumentRange? range);

		Assert.IsFalse(resolved);
		Assert.IsNull(range);
	}
}

[TestClass]
public class SemanticTokensDeltaParserTests
{
	[TestMethod]
	public void Parse_MissingStartReturnsEmptyDeltaPayload()
	{
		SemanticTokensWireResponse response = JsonSerializer.Deserialize<SemanticTokensWireResponse>(
			"""
			{
			  "resultId": "delta-1",
			  "edits": [
			    {
			      "deleteCount": 2,
			      "data": [1, 2, 3]
			    }
			  ]
			}
			""");

		SemanticTokensDeltaResponse result = SemanticTokensDeltaParser.Parse(response);

		Assert.AreEqual("delta-1", result.ResultId);
		Assert.IsNull(result.Data);
		Assert.IsNull(result.Edits);
	}
}
