namespace Nickelony.LanguageServer.Core.Tests;

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
