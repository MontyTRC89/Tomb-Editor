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
		LuaCompletionItemPayload baselineElement = CreateCompletionItem("baseline", kind: 6, detail: "variable", documentation: "plain text");
		LuaCompletionItemPayload boostedElement = CreateCompletionItem("boosted", kind: 6, detail: "local variable", documentation: "upvalue");

		LuaCompletionItem? baselineItem = LuaLanguageServerResponseParser.ParseCompletionItem(baselineElement, 0);
		LuaCompletionItem? boostedItem = LuaLanguageServerResponseParser.ParseCompletionItem(boostedElement, 0);

		Assert.IsNotNull(baselineItem);
		Assert.IsNotNull(boostedItem);
		Assert.AreEqual(35000.0, boostedItem.Priority - baselineItem.Priority);
	}

	[TestMethod]
	public void ParseCompletionItem_UsesParameterIconWhenDetailContainsParameter()
	{
		LuaCompletionItemPayload itemElement = CreateCompletionItem("arg", kind: 6, detail: "parameter", documentation: null);

		LuaCompletionItem? item = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, 0);

		Assert.IsNotNull(item);
		Assert.AreEqual(LuaCompletionIconKind.Parameter, item.IconKind);
	}

	[TestMethod]
	public void ParseCompletionItem_ParsesTextEditRange()
	{
		LuaCompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
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
		LuaCompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
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
		LuaCompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
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
		LuaCompletionItemPayload itemElement = DeserializeCompletionItemPayload(new
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
	public void ParseDefinitionLocation_UsesFirstEntryFromMultiLocationResponse()
	{
		string firstPath = Path.GetFullPath(@"C:\Workspace\Scripts\first.lua");
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\second.lua");

		LuaDefinitionLocation? location = LuaLanguageServerResponseParser.ParseDefinitionLocation(
			DeserializeDefinitionResponse(new object[]
			{
				new
				{
					uri = new Uri(firstPath).AbsoluteUri,
					range = new
					{
						start = new { line = 2, character = 4 },
						end = new { line = 2, character = 10 }
					}
				},
				new
				{
					uri = new Uri(secondPath).AbsoluteUri,
					range = new
					{
						start = new { line = 8, character = 1 },
						end = new { line = 8, character = 5 }
					}
				}
			}));

		Assert.IsNotNull(location);
		Assert.AreEqual(firstPath, location.FilePath);
		Assert.AreEqual(3, location.LineNumber);
		Assert.AreEqual(5, location.ColumnNumber);
	}

	[TestMethod]
	public void ParseDefinitionLocation_UsesTargetSelectionRangeFromLocationLink()
	{
		string targetPath = Path.GetFullPath(@"C:\Workspace\Scripts\linked.lua");

		LuaDefinitionLocation? location = LuaLanguageServerResponseParser.ParseDefinitionLocation(
			DeserializeDefinitionResponse(new
			{
				targetUri = new Uri(targetPath).AbsoluteUri,
				targetSelectionRange = new
				{
					start = new { line = 4, character = 2 },
					end = new { line = 4, character = 9 }
				}
			}));

		Assert.IsNotNull(location);
		Assert.AreEqual(targetPath, location.FilePath);
		Assert.AreEqual(5, location.LineNumber);
		Assert.AreEqual(3, location.ColumnNumber);
	}

	[TestMethod]
	public void ParseReferenceLocations_ParsesFileReferenceRanges()
	{
		string targetPath = Path.GetFullPath(@"C:\Workspace\Scripts\references.lua");

		IReadOnlyList<LuaReferenceLocation> locations = LuaLanguageServerResponseParser.ParseReferenceLocations(
			DeserializeReferenceResponse(new object[]
			{
				new
				{
					uri = new Uri(targetPath).AbsoluteUri,
					range = new
					{
						start = new { line = 2, character = 4 },
						end = new { line = 2, character = 9 }
					}
				},
				new
				{
					uri = "https://example.com/not-a-file.lua",
					range = new
					{
						start = new { line = 0, character = 0 },
						end = new { line = 0, character = 1 }
					}
				}
			}));

		Assert.AreEqual(1, locations.Count);
		Assert.AreEqual(targetPath, locations[0].FilePath);
		Assert.AreEqual(3, locations[0].Range.StartLineNumber);
		Assert.AreEqual(5, locations[0].Range.StartColumnNumber);
		Assert.AreEqual(3, locations[0].Range.EndLineNumber);
		Assert.AreEqual(10, locations[0].Range.EndColumnNumber);
	}

	[TestMethod]
	public void ParseWorkspaceEdit_MergesChangeMapAndDocumentChanges()
	{
		string firstPath = Path.GetFullPath(@"C:\Workspace\Scripts\first.lua");
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\second.lua");

		LuaWorkspaceEdit? workspaceEdit = LuaLanguageServerResponseParser.ParseWorkspaceEdit(
			DeserializeWorkspaceEditResponse(new
			{
				changes = new Dictionary<string, object[]>
				{
					[new Uri(firstPath).AbsoluteUri] =
					[
						new
						{
							range = new
							{
								start = new { line = 0, character = 0 },
								end = new { line = 0, character = 5 }
							},
							newText = "local"
						}
					]
				},
				documentChanges = new object[]
				{
					new
					{
						textDocument = new { uri = new Uri(secondPath).AbsoluteUri },
						edits = new object[]
						{
							new
							{
								range = new
								{
									start = new { line = 3, character = 1 },
									end = new { line = 3, character = 4 }
								},
								newText = "name"
							}
						}
					}
				}
			}));

		Assert.IsNotNull(workspaceEdit);
		Assert.AreEqual(2, workspaceEdit.DocumentEdits.Count);
		Assert.AreEqual(firstPath, workspaceEdit.DocumentEdits[0].FilePath);
		Assert.AreEqual("local", workspaceEdit.DocumentEdits[0].TextEdits[0].NewText);
		Assert.AreEqual(secondPath, workspaceEdit.DocumentEdits[1].FilePath);
		Assert.AreEqual("name", workspaceEdit.DocumentEdits[1].TextEdits[0].NewText);
	}

	[TestMethod]
	public void ParseDocumentFormattingEdits_ParsesFormattingTextEdits()
	{
		IReadOnlyList<LuaTextEdit> textEdits = LuaLanguageServerResponseParser.ParseDocumentFormattingEdits(
			DeserializeTextEdits(new object[]
			{
				new
				{
					range = new
					{
						start = new { line = 0, character = 0 },
						end = new { line = 0, character = 0 }
					},
					newText = "local value = 1\r\n"
				},
				new
				{
					range = new
					{
						start = new { line = 1, character = 0 },
						end = new { line = 1, character = 4 }
					},
					newText = "    "
				}
			}));

		Assert.AreEqual(2, textEdits.Count);
		Assert.AreEqual("local value = 1\r\n", textEdits[0].NewText);
		Assert.AreEqual(1, textEdits[0].Range.StartLineNumber);
		Assert.AreEqual(1, textEdits[0].Range.StartColumnNumber);
		Assert.AreEqual(2, textEdits[1].Range.StartLineNumber);
		Assert.AreEqual(5, textEdits[1].Range.EndColumnNumber);
	}

	[TestMethod]
	public void ParseSignatureHelp_UsesParameterLabelOffsetsAndActiveParameter()
	{
		LuaSignatureInfo? signatureInfo = LuaLanguageServerResponseParser.ParseSignatureHelp(
			DeserializeSignatureHelpResponse(new
			{
				activeSignature = 0,
				activeParameter = 1,
				signatures = new[]
				{
					new
					{
						label = "spawn(room, objectName)",
						documentation = new
						{
							kind = "markdown",
							value = "Spawns an object."
						},
						parameters = new object[]
						{
							new
							{
								label = new[] { 6, 10 },
								documentation = "Room id."
							},
							new
							{
								label = new[] { 12, 22 },
								documentation = "Object name."
							}
						}
					}
				}
			}));

		Assert.IsNotNull(signatureInfo);
		Assert.AreEqual("spawn(room, objectName)", signatureInfo.Label);
		Assert.AreEqual("Spawns an object.", signatureInfo.Documentation);
		Assert.AreEqual(1, signatureInfo.ActiveParameter);
		Assert.AreEqual(2, signatureInfo.Parameters.Count);
		Assert.AreEqual("objectName", signatureInfo.Parameters[1].Label);
		Assert.AreEqual("Object name.", signatureInfo.Parameters[1].Documentation);
	}

	[TestMethod]
	public void ParseSignatureHelp_UsesSignatureLevelActiveParameterWhenResponseOmitsIt()
	{
		LuaSignatureInfo? signatureInfo = LuaLanguageServerResponseParser.ParseSignatureHelp(
			DeserializeSignatureHelpResponse(new
			{
				signatures = new[]
				{
					new
					{
						label = "move(x, y)",
						activeParameter = 1,
						parameters = new object[]
						{
							new { label = "x" },
							new { label = "y" }
						}
					}
				}
			}));

		Assert.IsNotNull(signatureInfo);
		Assert.AreEqual(1, signatureInfo.ActiveParameter);
		Assert.AreEqual("y", signatureInfo.Parameters[1].Label);
	}

	private static LuaCompletionItemPayload CreateCompletionItem(string label, int kind, string? detail, string? documentation, string? insertText = null)
		=> DeserializeCompletionItemPayload(new Dictionary<string, object?>
		{
			["label"] = label,
			["kind"] = kind,
			["detail"] = detail,
			["documentation"] = documentation,
			["insertText"] = insertText ?? label,
			["filterText"] = label
		});

	private static LuaCompletionItemPayload DeserializeCompletionItemPayload(object payload)
		=> JsonSerializer.Deserialize<LuaCompletionItemPayload>(JsonSerializer.Serialize(payload))
			?? throw new InvalidOperationException("Failed to deserialize the Lua completion-item test payload.");

	private static LuaSignatureHelpResponse? DeserializeSignatureHelpResponse(object payload)
		=> JsonSerializer.Deserialize<LuaSignatureHelpResponse>(JsonSerializer.Serialize(payload));

	private static LuaWorkspaceEditResponse? DeserializeWorkspaceEditResponse(object payload)
		=> JsonSerializer.Deserialize<LuaWorkspaceEditResponse>(JsonSerializer.Serialize(payload));

	private static LuaTextEditPayload[]? DeserializeTextEdits(object payload)
		=> JsonSerializer.Deserialize<LuaTextEditPayload[]>(JsonSerializer.Serialize(payload));

	private static LuaDefinitionResponse DeserializeDefinitionResponse(object payload)
		=> JsonSerializer.Deserialize<LuaDefinitionResponse>(JsonSerializer.Serialize(payload));

	private static LuaReferenceResponse[]? DeserializeReferenceResponse(object payload)
		=> JsonSerializer.Deserialize<LuaReferenceResponse[]>(JsonSerializer.Serialize(payload));
}
