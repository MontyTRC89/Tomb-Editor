using Newtonsoft.Json.Schema;
using Nickelony.LanguageServer.Abstractions.Completion;
using TombLib.Scripting.Completion;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class GameFlowCompletionServiceTests
{
	private sealed class StubSchemaService(JSchema? schema) : IGameFlowSchemaService
	{
		public JSchema? Schema => schema;

		public SchemaKeywords? GetSchemaKeywords() => null;
	}

	[TestMethod]
	public void GetCompletionItems_EmptyWord_ReturnsSchemaItems()
	{
		var schema = new JSchema();
		schema.Properties["title"] = new JSchema { Type = JSchemaType.String };

		var service = new GameFlowCompletionService(new StubSchemaService(schema));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext(string.Empty, 0));

		Assert.IsTrue(items.Any(item => item.InsertText == "\"title\": "));
		Assert.IsTrue(items.Any(item => item.InsertText == "true"));
	}

	[TestMethod]
	public void GetCompletionItems_FiltersByWordAtCaret()
	{
		var schema = new JSchema();
		schema.Properties["title"] = new JSchema { Type = JSchemaType.String };
		schema.Properties["level"] = new JSchema { Type = JSchemaType.String };

		var service = new GameFlowCompletionService(new StubSchemaService(schema));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext("\"tit", 4));

		Assert.AreEqual(1, items.Count);
		Assert.AreEqual("\"title\": ", items[0].InsertText);
	}

	[TestMethod]
	public void GetCompletionItems_NullSchema_ReturnsEmpty()
	{
		var service = new GameFlowCompletionService(new StubSchemaService(null));

		IReadOnlyList<TextCompletionItem> items = service.GetCompletionItems(new TextCompletionContext("abc", 3));

		Assert.AreEqual(0, items.Count);
	}
}
