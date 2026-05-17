using NLog;
using NLog.Config;
using NLog.Targets;
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

	[TestMethod]
	public void ParseHoverInfo_PreservesIndentedMarkdownAndHardBreakWhitespace()
	{
		HoverResponse response = DeserializeHoverResponse(new
		{
			contents = new
			{
				kind = "markdown",
				value = "    local value = 1  \nnext"
			}
		});

		LuaHoverInfo? hover = LuaLanguageServerResponseParser.ParseHoverInfo(response);

		Assert.IsNotNull(hover);
		Assert.AreEqual("    local value = 1  \nnext", hover.Content);
		Assert.IsTrue(hover.IsMarkdown);
	}

	[TestMethod]
	public void ParseHoverInfo_CombinesMarkupArrayWithoutTrimmingIndentedMarkdownFragment()
	{
		HoverResponse response = DeserializeHoverResponse(new
		{
			contents = new object[]
			{
				"Summary",
				new
				{
					kind = "markdown",
					value = "    local value = 1"
				}
			}
		});

		LuaHoverInfo? hover = LuaLanguageServerResponseParser.ParseHoverInfo(response);

		Assert.IsNotNull(hover);
		Assert.AreEqual($"Summary{Environment.NewLine}{Environment.NewLine}    local value = 1", hover.Content);
		Assert.IsTrue(hover.IsMarkdown);
	}

	[TestMethod]
	public void ParseHoverInfo_CodeBlockPayloadUsesFenceLongerThanEmbeddedBackticks()
	{
		HoverResponse response = DeserializeHoverResponse(new
		{
			contents = new
			{
				language = "lua",
				value = "print(\"```\")"
			}
		});

		LuaHoverInfo? hover = LuaLanguageServerResponseParser.ParseHoverInfo(response);

		Assert.IsNotNull(hover);
		Assert.AreEqual("````lua\nprint(\"```\")\n````", hover.Content.Replace("\r\n", "\n", StringComparison.Ordinal));
		Assert.IsTrue(hover.IsMarkdown);
	}

	[TestMethod]
	public void MarkupContentReader_ExtractContent_CombinesMixedArrayAndSkipsMalformedEntries()
	{
		JsonElement element = JsonSerializer.SerializeToElement(new object[]
		{
			"Summary",
			new
			{
				kind = "markdown",
				value = "**bold**"
			},
			new
			{
				value = 5
			},
			new
			{
				language = "lua",
				value = "print(1)"
			},
			new
			{
				value = "tail"
			}
		});

		MarkupContent content = MarkupContentReader.ExtractContent(element);

		Assert.IsTrue(content.IsMarkdown);
		Assert.AreEqual(
			"Summary\n\n**bold**\n\n```lua\nprint(1)\n```\n\ntail",
			content.Text.Replace("\r\n", "\n", StringComparison.Ordinal));
	}

	[TestMethod]
	public void MarkupContentReader_ExtractContent_FallsBackToPlainValueWhenKindHasWrongType()
	{
		JsonElement element = JsonSerializer.SerializeToElement(new
		{
			kind = 5,
			value = "plain text"
		});

		MarkupContent content = MarkupContentReader.ExtractContent(element);

		Assert.AreEqual("plain text", content.Text);
		Assert.IsFalse(content.IsMarkdown);
	}

	[TestMethod]
	public void MarkupContentReader_ExtractContent_ReturnsDefaultForPartiallyMissingCodeBlockPayload()
	{
		JsonElement element = JsonSerializer.SerializeToElement(new
		{
			language = "lua"
		});

		MarkupContent content = MarkupContentReader.ExtractContent(element);

		Assert.IsTrue(string.IsNullOrEmpty(content.Text));
		Assert.IsFalse(content.IsMarkdown);
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
	public void DeserializeDefinitionResponse_PreservesAllTargetsFromMultiLocationResponse()
	{
		string firstPath = Path.GetFullPath(@"C:\Workspace\Scripts\first.lua");
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\second.lua");

		DefinitionResponse response = DeserializeDefinitionResponse(new object[]
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
		});

		Assert.AreEqual(2, response.Targets.Count);
		Assert.AreEqual(new Uri(firstPath).AbsoluteUri, response.Targets[0].Uri);
		Assert.AreEqual(3, response.Targets[0].LineNumber);
		Assert.AreEqual(5, response.Targets[0].ColumnNumber);
		Assert.AreEqual(new Uri(secondPath).AbsoluteUri, response.Targets[1].Uri);
		Assert.AreEqual(9, response.Targets[1].LineNumber);
		Assert.AreEqual(2, response.Targets[1].ColumnNumber);
	}

	[TestMethod]
	public void SerializeDefinitionResponse_WritesRoundTrippableLocationArray()
	{
		string firstUri = new Uri(Path.GetFullPath(@"C:\Workspace\Scripts\first.lua")).AbsoluteUri;
		string secondUri = new Uri(Path.GetFullPath(@"C:\Workspace\Scripts\second.lua")).AbsoluteUri;

		var response = new DefinitionResponse(
		[
			new DefinitionTargetResponse(firstUri, 3, 5),
			new DefinitionTargetResponse(secondUri, 9, 2)
		]);

		string json = JsonSerializer.Serialize(response);
		DefinitionResponse roundTripped = JsonSerializer.Deserialize<DefinitionResponse>(json)
			?? throw new AssertFailedException("Serialized definition response should deserialize successfully.");

		Assert.AreEqual(2, roundTripped.Targets.Count);
		Assert.AreEqual(firstUri, roundTripped.Targets[0].Uri);
		Assert.AreEqual(3, roundTripped.Targets[0].LineNumber);
		Assert.AreEqual(5, roundTripped.Targets[0].ColumnNumber);
		Assert.AreEqual(secondUri, roundTripped.Targets[1].Uri);
		Assert.AreEqual(9, roundTripped.Targets[1].LineNumber);
		Assert.AreEqual(2, roundTripped.Targets[1].ColumnNumber);
	}

	[TestMethod]
	public void DeserializeDefinitionResponse_IgnoresMalformedTargetsAndKeepsUsableEntries()
	{
		string validPath = Path.GetFullPath(@"C:\Workspace\Scripts\valid.lua");

		DefinitionResponse response = DeserializeDefinitionResponse(new object[]
		{
			new
			{
				uri = "not a uri",
				range = new
				{
					start = new { line = 0, character = 0 },
					end = new { line = 0, character = 1 }
				}
			},
			new
			{
				uri = new Uri(validPath).AbsoluteUri,
				range = new
				{
					start = new { line = 3, character = 2 },
					end = new { line = 3, character = 7 }
				}
			}
		});

		Assert.AreEqual(1, response.Targets.Count);
		Assert.AreEqual(new Uri(validPath).AbsoluteUri, response.Targets[0].Uri);
		Assert.AreEqual(4, response.Targets[0].LineNumber);
		Assert.AreEqual(3, response.Targets[0].ColumnNumber);
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
	public void DeserializeDefinitionResponse_FallsBackToTargetRangeWhenSelectionRangeIsMalformed()
	{
		string targetPath = Path.GetFullPath(@"C:\Workspace\Scripts\linked.lua");

		DefinitionResponse response = DeserializeDefinitionResponse(new
		{
			targetUri = new Uri(targetPath).AbsoluteUri,
			targetSelectionRange = new
			{
				start = new { line = -1, character = 2 },
				end = new { line = 4, character = 9 }
			},
			targetRange = new
			{
				start = new { line = 6, character = 3 },
				end = new { line = 6, character = 8 }
			}
		});

		Assert.AreEqual(1, response.Targets.Count);
		Assert.AreEqual(new Uri(targetPath).AbsoluteUri, response.Targets[0].Uri);
		Assert.AreEqual(7, response.Targets[0].LineNumber);
		Assert.AreEqual(4, response.Targets[0].ColumnNumber);
	}

	[TestMethod]
	public void DeserializeDefinitionResponse_ReturnsEmptyTargetsForSingleMalformedPayload()
	{
		DefinitionResponse response = DeserializeDefinitionResponse(new
		{
			uri = "not a uri",
			range = new
			{
				start = new { line = 0, character = 0 },
				end = new { line = 0, character = 1 }
			}
		});

		Assert.AreEqual(0, response.Targets.Count);
	}

	[TestMethod]
	public void ParseDefinitionLocation_ReturnsNullForNegativeTargetPosition()
	{
		string targetPath = Path.GetFullPath(@"C:\Workspace\Scripts\linked.lua");

		LuaDefinitionLocation? location = LuaLanguageServerResponseParser.ParseDefinitionLocation(
			DeserializeDefinitionResponse(new
			{
				targetUri = new Uri(targetPath).AbsoluteUri,
				targetSelectionRange = new
				{
					start = new { line = -1, character = 2 },
					end = new { line = 4, character = 9 }
				}
			}));

		Assert.IsNull(location);
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
	public void ParseReferenceLocations_IgnoresNegativeProtocolRanges()
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
						start = new { line = -1, character = 4 },
						end = new { line = 2, character = 9 }
					}
				}
			}));

		Assert.AreEqual(0, locations.Count);
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
	public void ParseWorkspaceEdit_ReturnsNullWhenDocumentChangesContainUnsupportedResourceOperation()
	{
		string firstPath = Path.GetFullPath(@"C:\Workspace\Scripts\first.lua");
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\second.lua");

		LuaWorkspaceEdit? workspaceEdit = LuaLanguageServerResponseParser.ParseWorkspaceEdit(
			DeserializeWorkspaceEditResponse(new
			{
				documentChanges = new object[]
				{
					new
					{
						textDocument = new { uri = new Uri(firstPath).AbsoluteUri },
						edits = new object[]
						{
							new
							{
								range = new
								{
									start = new { line = 0, character = 0 },
									end = new { line = 0, character = 5 }
								},
								newText = "local"
							}
						}
					},
					new
					{
						kind = "rename",
						oldUri = new Uri(firstPath).AbsoluteUri,
						newUri = new Uri(secondPath).AbsoluteUri
					}
				}
			}));

		Assert.IsNull(workspaceEdit);
	}

	[TestMethod]
	public void DeserializeWorkspaceEditResponse_PreservesResourceOperationMetadataInDocumentChanges()
	{
		string firstPath = Path.GetFullPath(@"C:\Workspace\Scripts\first.lua");
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\second.lua");

		WorkspaceEditResponse? response = DeserializeWorkspaceEditResponse(new
		{
			documentChanges = new object[]
			{
				new
				{
					textDocument = new { uri = new Uri(firstPath).AbsoluteUri },
					edits = new object[]
					{
						new
						{
							range = new
							{
								start = new { line = 0, character = 0 },
								end = new { line = 0, character = 5 }
							},
							newText = "local"
						}
					}
				},
				new
				{
					kind = "rename",
					oldUri = new Uri(firstPath).AbsoluteUri,
					newUri = new Uri(secondPath).AbsoluteUri
				}
			}
		});

		Assert.IsNotNull(response);
		Assert.IsNotNull(response.Value.DocumentChanges);
		Assert.AreEqual(2, response.Value.DocumentChanges.Count);
		Assert.AreEqual(firstPath, Path.GetFullPath(new Uri(response.Value.DocumentChanges[0].TextDocument?.Uri ?? string.Empty).LocalPath));
		Assert.AreEqual("rename", response.Value.DocumentChanges[1].Kind);
		Assert.AreEqual(new Uri(firstPath).AbsoluteUri, response.Value.DocumentChanges[1].OldUri);
		Assert.AreEqual(new Uri(secondPath).AbsoluteUri, response.Value.DocumentChanges[1].NewUri);
	}

	[TestMethod]
	public void WorkspaceEditResponse_DefensivelyClonesNestedEditCollections()
	{
		IReadOnlyList<TextEditPayload> edits =
		[
			new TextEditPayload(
				new ProtocolRangePayload(
					new ProtocolNullablePosition(0, 0),
					new ProtocolNullablePosition(0, 1)),
				"x")
		];
		var changes = new Dictionary<string, IReadOnlyList<TextEditPayload>?>
		{
			[new Uri(@"C:\Workspace\Scripts\first.lua").AbsoluteUri] = edits
		};
		WorkspaceDocumentChangePayload[] documentChanges =
		[
			new WorkspaceDocumentChangePayload(
				new TextDocumentUriPayload(new Uri(@"C:\Workspace\Scripts\first.lua").AbsoluteUri),
				edits,
				kind: null,
				uri: null,
				oldUri: null,
				newUri: null)
		];

		var response = new WorkspaceEditResponse(changes, documentChanges);
		changes.Clear();
		documentChanges[0] = new WorkspaceDocumentChangePayload(
			new TextDocumentUriPayload(new Uri(@"C:\Workspace\Scripts\second.lua").AbsoluteUri),
			edits,
			kind: "rename",
			uri: null,
			oldUri: new Uri(@"C:\Workspace\Scripts\first.lua").AbsoluteUri,
			newUri: new Uri(@"C:\Workspace\Scripts\second.lua").AbsoluteUri);

		Assert.IsNotNull(response.Changes);
		Assert.AreEqual(1, response.Changes.Count);
		Assert.IsNotNull(response.DocumentChanges);
		Assert.AreEqual(1, response.DocumentChanges.Count);
		Assert.AreEqual(new Uri(@"C:\Workspace\Scripts\first.lua").AbsoluteUri, response.DocumentChanges[0].TextDocument?.Uri);
		Assert.IsFalse(response.DocumentChanges[0].IsResourceOperation);
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

	private static CompletionItemPayload CreateCompletionItem(string label, int kind, string? detail, string? documentation, string? insertText = null)
		=> DeserializeCompletionItemPayload(new Dictionary<string, object?>
		{
			["label"] = label,
			["kind"] = kind,
			["detail"] = detail,
			["documentation"] = documentation,
			["insertText"] = insertText ?? label,
			["filterText"] = label
		});

	private static CompletionItemPayload DeserializeCompletionItemPayload(object payload)
		=> JsonSerializer.Deserialize<CompletionItemPayload>(JsonSerializer.Serialize(payload))
			?? throw new InvalidOperationException("Failed to deserialize the Lua completion-item test payload.");

	private static CompletionResponse? DeserializeCompletionResponse(object payload)
		=> JsonSerializer.Deserialize<CompletionResponse>(JsonSerializer.Serialize(payload));

	private sealed class NLogMemoryScope : IDisposable
	{
		private readonly LoggingConfiguration? _previousConfiguration;

		public NLogMemoryScope(LogLevel minLevel)
		{
			_previousConfiguration = LogManager.Configuration;

			var target = new MemoryTarget("LuaLanguageServerResponseParserTests")
			{
				Layout = "${level}|${message}|${exception:format=Message}"
			};

			var configuration = new LoggingConfiguration();
			configuration.AddTarget(target);
			configuration.AddRule(minLevel, LogLevel.Fatal, target);

			LogManager.Configuration = configuration;
			LogManager.ReconfigExistingLoggers();

			Target = target;
		}

		public MemoryTarget Target { get; }

		public IList<string> Logs => Target.Logs;

		public void Dispose()
		{
			LogManager.Configuration = _previousConfiguration;
			LogManager.ReconfigExistingLoggers();
		}
	}

	private static HoverResponse DeserializeHoverResponse(object payload)
		=> JsonSerializer.Deserialize<HoverResponse>(JsonSerializer.Serialize(payload))
			?? throw new InvalidOperationException("Failed to deserialize the hover response test payload.");

	private static SignatureHelpResponse? DeserializeSignatureHelpResponse(object payload)
		=> JsonSerializer.Deserialize<SignatureHelpResponse>(JsonSerializer.Serialize(payload));

	private static WorkspaceEditResponse? DeserializeWorkspaceEditResponse(object payload)
		=> JsonSerializer.Deserialize<WorkspaceEditResponse>(JsonSerializer.Serialize(payload));

	private static TextEditPayload[]? DeserializeTextEdits(object payload)
		=> JsonSerializer.Deserialize<TextEditPayload[]>(JsonSerializer.Serialize(payload));

	private static DefinitionResponse DeserializeDefinitionResponse(object payload)
		=> JsonSerializer.Deserialize<DefinitionResponse>(JsonSerializer.Serialize(payload))
			?? throw new InvalidOperationException("Failed to deserialize the definition response test payload.");

	private static ReferenceResponse[]? DeserializeReferenceResponse(object payload)
		=> JsonSerializer.Deserialize<ReferenceResponse[]>(JsonSerializer.Serialize(payload));
}
