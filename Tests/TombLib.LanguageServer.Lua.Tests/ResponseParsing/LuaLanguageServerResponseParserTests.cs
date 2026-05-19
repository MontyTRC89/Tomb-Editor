using NLog;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua.Tests;

[TestClass]
public partial class LuaLanguageServerResponseParserTests
{
	[TestMethod]
	public void ParseCompletionItem_AddsLocalAndUpvaluePriorityBonuses()
	{
		CompletionItemPayload baselineElement = CreateCompletionItem("baseline", kind: 6, detail: "variable", documentation: "plain text");
		CompletionItemPayload boostedElement = CreateCompletionItem("boosted", kind: 6, detail: "local variable", documentation: "upvalue");

		LuaCompletionItem? baselineItem = LuaLanguageServerResponseParser.ParseCompletionItem(baselineElement, 0);
		LuaCompletionItem? boostedItem = LuaLanguageServerResponseParser.ParseCompletionItem(boostedElement, 0);

		Assert.IsNotNull(baselineItem);
		Assert.IsNotNull(boostedItem);
		Assert.AreEqual(35000.0, boostedItem.Priority - baselineItem.Priority);
	}

	[TestMethod]
	public void ParseCompletionItem_UsesParameterIconWhenDetailContainsParameter()
	{
		CompletionItemPayload itemElement = CreateCompletionItem("arg", kind: 6, detail: "parameter", documentation: null);

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.AreEqual(LuaCompletionIconKind.Parameter, item.IconKind);
	}

	[TestMethod]
	public void ParseCompletionItem_ParsesTextEditRange()
	{
		CompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
		{
			label = "print",
			kind = 3,
			textEdit = new
			{
				newText = "print",
				range = new
				{
					start = new { line = 1, character = 2 },
					end = new { line = 1, character = 5 }
				}
			}
		});

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.AreEqual("print", item.InsertText);
		Assert.IsNotNull(item.TextEdit);
		Assert.AreEqual(new LuaCompletionPosition(1, 2), item.TextEdit.Value.InsertRange.Start);
		Assert.AreEqual(new LuaCompletionPosition(1, 5), item.TextEdit.Value.InsertRange.End);
		Assert.IsNull(item.TextEdit.Value.ReplaceRange);
	}

	[TestMethod]
	public void ParseCompletionItem_ParsesInsertReplaceEditRanges()
	{
		CompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
		{
			label = "print",
			kind = 3,
			textEdit = new
			{
				newText = "print",
				insert = new
				{
					start = new { line = 0, character = 1 },
					end = new { line = 0, character = 3 }
				},
				replace = new
				{
					start = new { line = 0, character = 1 },
					end = new { line = 0, character = 6 }
				}
			}
		});

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.IsNotNull(item.TextEdit);
		Assert.AreEqual(new LuaCompletionPosition(0, 1), item.TextEdit.Value.InsertRange.Start);
		Assert.AreEqual(new LuaCompletionPosition(0, 3), item.TextEdit.Value.InsertRange.End);
		Assert.AreEqual(new LuaCompletionPosition(0, 1), item.TextEdit.Value.ReplaceRange!.Value.Start);
		Assert.AreEqual(new LuaCompletionPosition(0, 6), item.TextEdit.Value.ReplaceRange!.Value.End);
		Assert.AreEqual(new LuaCompletionPosition(0, 6), item.TextEdit.Value.ReplacementRange.End);
	}

	[TestMethod]
	public void ParseCompletionItem_StripsSnippetAndPreservesFinalCaretOffset()
	{
		CompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
		{
			label = "if",
			kind = 15,
			insertText = "if ${1:condition} then\r\n\t$0\r\nend",
			insertTextFormat = 2
		});

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.AreEqual("if condition then\r\n\t\r\nend", item.InsertText);
		Assert.AreEqual("if condition then\r\n\t".Length, item.InsertCaretOffset);
	}

	[TestMethod]
	public void ParseCompletionItem_PreservesUnknownSnippetPlaceholdersAndPlacesCaretAfterDefaultText()
	{
		CompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
		{
			label = "call",
			kind = 3,
			insertText = "call(${name}, ${0:done})",
			insertTextFormat = 2
		});

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.AreEqual("call(${name}, done)", item.InsertText);
		Assert.AreEqual("call(${name}, done".Length, item.InsertCaretOffset);
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

	[TestMethod]
	public void ParseCompletionItems_PreservesDistinctItemsWithDifferentTextEdits()
	{
		IReadOnlyList<LuaCompletionItem> items = LuaLanguageServerResponseParser.ParseCompletionItems(
			[
				DeserializeCompletionItemPayload(new
				{
					label = "spawn",
					kind = 3,
					insertText = "spawn",
					textEdit = new
					{
						newText = "spawn",
						range = new
						{
							start = new { line = 0, character = 0 },
							end = new { line = 0, character = 3 }
						}
					}
				}),
				DeserializeCompletionItemPayload(new
				{
					label = "spawn",
					kind = 3,
					insertText = "spawn",
					textEdit = new
					{
						newText = "spawn",
						range = new
						{
							start = new { line = 0, character = 1 },
							end = new { line = 0, character = 4 }
						}
					}
				})
			]);

		Assert.AreEqual(2, items.Count);
		Assert.AreEqual(new LuaCompletionPosition(0, 0), items[0].TextEdit?.InsertRange.Start);
		Assert.AreEqual(new LuaCompletionPosition(0, 1), items[1].TextEdit?.InsertRange.Start);
	}

	[TestMethod]
	public void DeserializeCompletionResponse_PreservesCompletionListMetadata()
	{
		CompletionResponse? response = DeserializeCompletionResponse(new
		{
			isIncomplete = true,
			items = new object[]
			{
				new
				{
					label = "spawn",
					kind = 3,
					insertText = "spawn",
					filterText = "spawn"
				}
			}
		});

		Assert.IsNotNull(response);
		Assert.IsTrue(response.IsIncomplete);
		Assert.IsNotNull(response.Items);
		Assert.AreEqual(1, response.Items.Count);
		Assert.AreEqual("spawn", response.Items[0].Label);
	}

	[TestMethod]
	public void DeserializeCompletionResponse_ParsesArrayPayload()
	{
		CompletionResponse? response = DeserializeCompletionResponse(new object[]
		{
			new
			{
				label = "spawn",
				kind = 3,
				insertText = "spawn",
				filterText = "spawn"
			}
		});

		Assert.IsNotNull(response);
		Assert.IsFalse(response.IsIncomplete);
		Assert.IsNotNull(response.Items);
		Assert.AreEqual(1, response.Items.Count);
		Assert.AreEqual("spawn", response.Items[0].Label);
	}

	[TestMethod]
	public void DeserializeCompletionResponse_NullPayload_ReturnsEmptyResponse()
	{
		CompletionResponse? response = JsonSerializer.Deserialize<CompletionResponse>("null");
		Assert.IsNull(response);
	}

	[TestMethod]
	public void DeserializeCompletionResponse_IgnoresNonBooleanIncompleteFlag()
	{
		CompletionResponse? response = DeserializeCompletionResponse(new
		{
			isIncomplete = "yes",
			items = new object[]
			{
				new
				{
					label = "spawn",
					kind = 3,
					insertText = "spawn",
					filterText = "spawn"
				}
			}
		});

		Assert.IsNotNull(response);
		Assert.IsFalse(response.IsIncomplete);
		Assert.IsNotNull(response.Items);
		Assert.AreEqual(1, response.Items.Count);
	}

	[TestMethod]
	public void CompletionResponse_DefensivelyClonesItemList()
	{
		CompletionItemPayload[] items =
		[
			new CompletionItemPayload
			{
				Label = "spawn",
				Kind = 3,
				InsertText = "spawn"
			}
		];

		var response = new CompletionResponse(items);
		items[0] = new CompletionItemPayload
		{
			Label = "changed",
			Kind = 14,
			InsertText = "changed"
		};

		Assert.IsNotNull(response.Items);
		Assert.AreEqual(1, response.Items.Count);
		Assert.AreEqual("spawn", response.Items[0].Label);
	}

	[TestMethod]
	public void DeserializeCompletionResponse_IgnoresMalformedCompletionListItemsShape()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Warn);

		CompletionResponse? response = DeserializeCompletionResponse(new
		{
			isIncomplete = true,
			items = new
			{
				label = "spawn"
			}
		});

		Assert.IsNotNull(response);
		Assert.IsNull(response.Items);
		Assert.IsFalse(response.IsIncomplete);
		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("unsupported JSON kind", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("Object", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void DeserializeCompletionResponse_LogsWhenCompletionListItemsPropertyIsMissing()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Warn);

		CompletionResponse? response = DeserializeCompletionResponse(new
		{
			isIncomplete = true
		});

		Assert.IsNotNull(response);
		Assert.IsNull(response.Items);
		Assert.IsFalse(response.IsIncomplete);
		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("items' property was missing", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void SerializeCompletionResponse_WritesRoundTrippableCompletionListShape()
	{
		var response = new CompletionResponse(
			[
				new CompletionItemPayload
				{
					Label = "spawn",
					Kind = 3,
					InsertText = "spawn"
				}
			],
			isIncomplete: true);

		string json = JsonSerializer.Serialize(response);
		CompletionResponse? roundTripped = JsonSerializer.Deserialize<CompletionResponse>(json);

		Assert.AreEqual("{\"isIncomplete\":true,\"items\":[{\"label\":\"spawn\",\"kind\":3,\"insertText\":\"spawn\"}]}", json);
		Assert.IsNotNull(roundTripped);
		Assert.IsTrue(roundTripped.IsIncomplete);
		Assert.IsNotNull(roundTripped.Items);
		Assert.AreEqual(1, roundTripped.Items.Count);
		Assert.AreEqual("spawn", roundTripped.Items[0].Label);
	}

	[TestMethod]
	public void ParseCompletionItem_PreservesMarkdownIndentedCodeBlockDocumentation()
	{
		CompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
		{
			label = "spawn",
			kind = 3,
			documentation = new
			{
				kind = "markdown",
				value = "    local value = 1"
			}
		});

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.AreEqual("    local value = 1", item.Description);
		Assert.IsTrue(item.IsDescriptionMarkdown);
	}
}
