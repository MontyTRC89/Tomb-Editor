namespace Nickelony.LanguageServer.Core.Tests;

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
