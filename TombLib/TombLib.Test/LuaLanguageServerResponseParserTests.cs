using System.Text.Json;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Test;

[TestClass]
public class LuaLanguageServerResponseParserTests
{
	[TestMethod]
	public void ParseCompletionItem_AddsLocalAndUpvaluePriorityBonuses()
	{
		JsonElement baselineElement = CreateCompletionItem("baseline", kind: 6, detail: "variable", documentation: "plain text");
		JsonElement boostedElement = CreateCompletionItem("boosted", kind: 6, detail: "local variable", documentation: "upvalue");

		LuaCompletionItem? baselineItem = LuaLanguageServerResponseParser.ParseCompletionItem(baselineElement, 0);
		LuaCompletionItem? boostedItem = LuaLanguageServerResponseParser.ParseCompletionItem(boostedElement, 0);

		Assert.IsNotNull(baselineItem);
		Assert.IsNotNull(boostedItem);
		Assert.AreEqual(35000.0, boostedItem.Priority - baselineItem.Priority);
	}

	[TestMethod]
	public void ParseCompletionItem_UsesParameterIconWhenDetailContainsParameter()
	{
		JsonElement itemElement = CreateCompletionItem("arg", kind: 6, detail: "parameter", documentation: null);

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.AreEqual(LuaCompletionIconKind.Parameter, item.IconKind);
	}

	[TestMethod]
	public void ParseCompletionItems_DeduplicatesLabelAndInsertTextCaseSensitively()
	{
		IReadOnlyList<LuaCompletionItem> items = LuaLanguageServerResponseParser.ParseCompletionItems(
			[
				CreateCompletionItem("Value", kind: 6, detail: "variable", documentation: null, insertText: "Value"),
				CreateCompletionItem("value", kind: 6, detail: "variable", documentation: null, insertText: "value"),
				CreateCompletionItem("Value", kind: 6, detail: "variable", documentation: null, insertText: "Value")
			]);

		// Lua is case-sensitive: "Value" and "value" are distinct symbols, but the duplicate "Value"
		// must still collapse so the popup does not show the same entry twice.
		Assert.AreEqual(2, items.Count);
		Assert.AreEqual("Value", items[0].Label);
		Assert.AreEqual("value", items[1].Label);
	}

	private static JsonElement CreateCompletionItem(string label, int kind, string? detail, string? documentation, string? insertText = null)
		=> JsonSerializer.SerializeToElement(new Dictionary<string, object?>
		{
			["label"] = label,
			["kind"] = kind,
			["detail"] = detail,
			["documentation"] = documentation,
			["insertText"] = insertText ?? label,
			["filterText"] = label
		});
}
