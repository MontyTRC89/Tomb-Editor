using Nickelony.LanguageServer.Abstractions.Completion;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Extensions;

namespace TombLib.Tests;

[TestClass]
public class TextCompletionFilterTests
{
	private static readonly IReadOnlyList<TextCompletionItem> Items =
	[
		new TextCompletionItem("Alpha", "Alpha"),
		new TextCompletionItem("Beta", "Beta: "),
		new TextCompletionItem("Gamma", "Gamma")
	];

	[TestMethod]
	public void FilterByCurrentWord_EmptyWord_ReturnsAllItems()
	{
		IReadOnlyList<TextCompletionItem> result = TextCompletionFilter.FilterByCurrentWord(Items, new TextCompletionContext(string.Empty, 0));

		Assert.AreEqual(3, result.Count);
	}

	[TestMethod]
	public void FilterByCurrentWord_KeepsItemsContainingWord()
	{
		IReadOnlyList<TextCompletionItem> result = TextCompletionFilter.FilterByCurrentWord(Items, new TextCompletionContext("Bet", 3));

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("Beta: ", result[0].InsertText);
	}

	[TestMethod]
	public void FilterByCurrentWord_IsCaseInsensitive()
	{
		IReadOnlyList<TextCompletionItem> result = TextCompletionFilter.FilterByCurrentWord(Items, new TextCompletionContext("gam", 3));

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("Gamma", result[0].InsertText);
	}

	[TestMethod]
	public void FilterByCurrentWord_NoMatch_ReturnsEmpty()
	{
		IReadOnlyList<TextCompletionItem> result = TextCompletionFilter.FilterByCurrentWord(Items, new TextCompletionContext("Zzz", 3));

		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void GetIdentifierPrefix_ReturnsPrefixAtCaret()
	{
		Assert.AreEqual("Beta_2", "Beta_2: value".GetIdentifierPrefix(6));
	}

	[TestMethod]
	public void GetIdentifierPrefix_InvalidCaret_ReturnsEmpty()
	{
		Assert.AreEqual(string.Empty, "Beta".GetIdentifierPrefix(5));
	}
}
