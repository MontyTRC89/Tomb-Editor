using Nickelony.LanguageServer.Abstractions.Completion;
using TombLib.Scripting.Completion;
using TombLib.Scripting.GameFlowScript.Completion;

namespace TombLib.Tests;

[TestClass]
public class GameFlowCompletionProviderTests
{
	private static GameFlowCompletionProvider CreateProvider() => new();

	[TestMethod]
	public void GetCompletionItems_EmptyWord_ReturnsAllCatalogItems()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionContext(string.Empty, 0));

		Assert.IsTrue(items.Count > 0);
		Assert.IsTrue(items.Any(item => item.InsertText == "LEVEL: "));
		Assert.IsTrue(items.Any(item => item.InsertText == "DESCRIPTION: "));
		Assert.IsTrue(items.Any(item => item.InsertText == "EXIT_TO_TITLE"));
	}

	[TestMethod]
	public void GetCompletionItems_CaretAtDocumentStart_ReturnsAllCatalogItems()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionContext("LEVEL: ", 0));

		Assert.IsTrue(items.Count > 0);
	}

	[TestMethod]
	public void GetCompletionItems_FiltersByWordTypedAtCaret()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionContext("TITLE", 5));

		Assert.IsTrue(items.Count > 0);
		Assert.IsTrue(items.All(item => item.InsertText.Contains("TITLE", StringComparison.OrdinalIgnoreCase)));
		Assert.IsTrue(items.Any(item => item.InsertText == "TITLE: "));
	}

	[TestMethod]
	public void GetCompletionItems_NonMatchingWord_ReturnsEmpty()
	{
		IReadOnlyList<TextCompletionItem> items = CreateProvider().GetCompletionItems(new TextCompletionContext("QwErTyZz", 8));

		Assert.AreEqual(0, items.Count);
	}
}
