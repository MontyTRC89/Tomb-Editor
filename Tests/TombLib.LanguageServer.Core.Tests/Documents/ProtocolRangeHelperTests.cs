namespace Nickelony.LanguageServer.Core.Tests;

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
