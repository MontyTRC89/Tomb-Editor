using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

[TestClass]
public class LuaIncrementalEditCalculatorTests
{
	[TestMethod]
	public void Compute_CollapsesUnchangedPrefixAndSuffixIntoMinimalRangeEdit()
	{
		const string oldText = "local foo = 1\nlocal bar = 2\n";
		const string newText = "local foo = 1\nlocal baz = 2\n";

		LuaDocumentChangeRange range = LuaIncrementalEditCalculator.Compute(oldText, newText, LuaDocumentLineOffsets.Build(oldText));

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

		LuaDocumentChangeRange range = LuaIncrementalEditCalculator.Compute(oldText, newText, LuaDocumentLineOffsets.Build(oldText));

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

		LuaDocumentChangeRange range = LuaIncrementalEditCalculator.Compute(text, text, LuaDocumentLineOffsets.Build(text));

		Assert.AreEqual(string.Empty, range.Text);
		Assert.AreEqual(range.StartLine, range.EndLine);
		Assert.AreEqual(range.StartCharacter, range.EndCharacter);
	}
}

[TestClass]
public class LuaLanguageServerSemanticTokensDeltaParserTests
{
	[TestMethod]
	public void ApplyEdits_ReplacesContiguousRangeAndPreservesSurroundingData()
	{
		int[] previous = [0, 0, 5, 1, 0, 0, 6, 3, 2, 0, 1, 0, 4, 1, 0];
		LuaSemanticTokensEdit edit = new(Start: 5, DeleteCount: 5, Data: [0, 6, 4, 2, 0]);

		int[]? result = LuaLanguageServerSemanticTokensDeltaParser.ApplyEdits(previous, [edit]);

		Assert.IsNotNull(result);
		CollectionAssert.AreEqual(new[] { 0, 0, 5, 1, 0, 0, 6, 4, 2, 0, 1, 0, 4, 1, 0 }, result);
	}

	[TestMethod]
	public void ApplyEdits_ReturnsNullWhenEditOutOfRange()
	{
		int[] previous = [0, 0, 1, 0, 0];
		LuaSemanticTokensEdit edit = new(Start: 4, DeleteCount: 5, Data: []);

		int[]? result = LuaLanguageServerSemanticTokensDeltaParser.ApplyEdits(previous, [edit]);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void ApplyEdits_AppliesMultipleEditsInAscendingOrder()
	{
		int[] previous = [10, 11, 12, 13, 14];
		LuaSemanticTokensEdit insertHead = new(Start: 0, DeleteCount: 0, Data: [99]);
		LuaSemanticTokensEdit replaceTail = new(Start: 4, DeleteCount: 1, Data: [44, 45]);

		int[]? result = LuaLanguageServerSemanticTokensDeltaParser.ApplyEdits(previous, [replaceTail, insertHead]);

		Assert.IsNotNull(result);
		CollectionAssert.AreEqual(new[] { 99, 10, 11, 12, 13, 44, 45 }, result);
	}
}
