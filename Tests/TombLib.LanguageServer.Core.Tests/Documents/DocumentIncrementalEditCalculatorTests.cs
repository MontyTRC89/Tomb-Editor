namespace Nickelony.LanguageServer.Core.Tests;

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
