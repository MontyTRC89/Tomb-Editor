using System.Text.Json;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

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
	public void Parse_MissingStartRejectsMalformedDeltaEdit()
	{
		SemanticTokensWireResponse response = JsonSerializer.Deserialize<SemanticTokensWireResponse>(
			"""
			{
			  "resultId": "delta-1",
			  "edits": [
			    { "deleteCount": 2, "data": [9, 8] }
			  ]
			}
			""");

		SemanticTokensDeltaResponse parsed = SemanticTokensDeltaParser.Parse(response);

		Assert.IsNull(parsed.Data);
		Assert.IsNull(parsed.Edits);
	}

	[TestMethod]
	public void Parse_MissingDeleteCountRejectsMalformedDeltaEdit()
	{
		SemanticTokensWireResponse response = JsonSerializer.Deserialize<SemanticTokensWireResponse>(
			"""
			{
			  "resultId": "delta-2",
			  "edits": [
			    { "start": 4, "data": [7] }
			  ]
			}
			""");

		SemanticTokensDeltaResponse parsed = SemanticTokensDeltaParser.Parse(response);

		Assert.IsNull(parsed.Data);
		Assert.IsNull(parsed.Edits);
	}

	[TestMethod]
	public void Parse_MissingDataTreatsEditAsDeleteOnlyPerProtocol()
	{
		SemanticTokensWireResponse response = JsonSerializer.Deserialize<SemanticTokensWireResponse>(
			"""
			{
			  "resultId": "delta-3",
			  "edits": [
			    { "start": 4, "deleteCount": 1 }
			  ]
			}
			""");

		SemanticTokensDeltaResponse parsed = SemanticTokensDeltaParser.Parse(response);

		Assert.IsNull(parsed.Data);
		Assert.IsNotNull(parsed.Edits);
		Assert.AreEqual(1, parsed.Edits.Count);
		Assert.AreEqual(4, parsed.Edits[0].Start);
		Assert.AreEqual(1, parsed.Edits[0].DeleteCount);
		Assert.AreEqual(0, parsed.Edits[0].Data.Length);
	}

	[TestMethod]
	public void Parse_NegativeStartRejectsMalformedDeltaEdit()
	{
		SemanticTokensWireResponse response = JsonSerializer.Deserialize<SemanticTokensWireResponse>(
			"""
			{
			  "resultId": "delta-4",
			  "edits": [
			    { "start": -1, "deleteCount": 1, "data": [7] }
			  ]
			}
			""");

		SemanticTokensDeltaResponse parsed = SemanticTokensDeltaParser.Parse(response);

		Assert.IsNull(parsed.Data);
		Assert.IsNull(parsed.Edits);
	}

	[TestMethod]
	public void Parse_NegativeDeleteCountRejectsMalformedDeltaEdit()
	{
		SemanticTokensWireResponse response = JsonSerializer.Deserialize<SemanticTokensWireResponse>(
			"""
			{
			  "resultId": "delta-5",
			  "edits": [
			    { "start": 1, "deleteCount": -1, "data": [7] }
			  ]
			}
			""");

		SemanticTokensDeltaResponse parsed = SemanticTokensDeltaParser.Parse(response);

		Assert.IsNull(parsed.Data);
		Assert.IsNull(parsed.Edits);
	}

	[TestMethod]
	public void ApplyEdits_ReplacesContiguousRangeAndPreservesSurroundingData()
	{
		int[] previous = [0, 0, 5, 1, 0, 0, 6, 3, 2, 0, 1, 0, 4, 1, 0];
		SemanticTokensEdit edit = new(Start: 5, DeleteCount: 5, Data: [0, 6, 4, 2, 0]);

		int[]? result = SemanticTokensDeltaParser.ApplyEdits(previous, [edit]);

		Assert.IsNotNull(result);
		CollectionAssert.AreEqual(new[] { 0, 0, 5, 1, 0, 0, 6, 4, 2, 0, 1, 0, 4, 1, 0 }, result);
	}

	[TestMethod]
	public void ApplyEdits_ReturnsNullWhenEditOutOfRange()
	{
		int[] previous = [0, 0, 1, 0, 0];
		SemanticTokensEdit edit = new(Start: 4, DeleteCount: 5, Data: []);

		int[]? result = SemanticTokensDeltaParser.ApplyEdits(previous, [edit]);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void ApplyEdits_AppliesMultipleEditsInAscendingOrder()
	{
		int[] previous = [10, 11, 12, 13, 14];
		SemanticTokensEdit insertHead = new(Start: 0, DeleteCount: 0, Data: [99]);
		SemanticTokensEdit replaceTail = new(Start: 4, DeleteCount: 1, Data: [44, 45]);

		int[]? result = SemanticTokensDeltaParser.ApplyEdits(previous, [insertHead, replaceTail]);

		Assert.IsNotNull(result);
		CollectionAssert.AreEqual(new[] { 99, 10, 11, 12, 13, 44, 45 }, result);
	}

	[TestMethod]
	public void ApplyEdits_ReturnsNullWhenEditsMoveBackward()
	{
		int[] previous = [10, 11, 12, 13, 14];
		SemanticTokensEdit replaceTail = new(Start: 4, DeleteCount: 1, Data: [44, 45]);
		SemanticTokensEdit insertHead = new(Start: 0, DeleteCount: 0, Data: [99]);

		int[]? result = SemanticTokensDeltaParser.ApplyEdits(previous, [replaceTail, insertHead]);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void ApplyEdits_ReturnsNullWhenEditsOverlap()
	{
		int[] previous = [10, 11, 12, 13, 14];
		SemanticTokensEdit firstEdit = new(Start: 1, DeleteCount: 2, Data: [99]);
		SemanticTokensEdit overlappingEdit = new(Start: 2, DeleteCount: 1, Data: [55]);

		int[]? result = SemanticTokensDeltaParser.ApplyEdits(previous, [firstEdit, overlappingEdit]);

		Assert.IsNull(result);
	}
}
