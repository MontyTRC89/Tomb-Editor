using NLog;
using System.Text.Json;
using TombLib.Scripting.Editing;
using TombLib.Scripting.Navigation;

namespace TombLib.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerResponseParserTests
{
	[TestMethod]
	public void ParseDefinitionLocation_UsesFirstEntryFromMultiLocationResponse()
	{
		string firstPath = Path.GetFullPath(@"C:\Workspace\Scripts\first.lua");
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\second.lua");

		TextDefinitionLocation? location = LuaLanguageServerResponseParser.ParseDefinitionLocation(
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

		TextDefinitionLocation? location = LuaLanguageServerResponseParser.ParseDefinitionLocation(
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

		TextDefinitionLocation? location = LuaLanguageServerResponseParser.ParseDefinitionLocation(
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

		IReadOnlyList<TextReferenceLocation> locations = LuaLanguageServerResponseParser.ParseReferenceLocations(
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
		Assert.AreEqual(3, locations[0].StartLineNumber);
		Assert.AreEqual(5, locations[0].StartColumnNumber);
		Assert.AreEqual(3, locations[0].EndLineNumber);
		Assert.AreEqual(10, locations[0].EndColumnNumber);
	}

	[TestMethod]
	public void ParseReferenceLocations_IgnoresNegativeProtocolRanges()
	{
		string targetPath = Path.GetFullPath(@"C:\Workspace\Scripts\references.lua");

		IReadOnlyList<TextReferenceLocation> locations = LuaLanguageServerResponseParser.ParseReferenceLocations(
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

		TextWorkspaceEdit? workspaceEdit = LuaLanguageServerResponseParser.ParseWorkspaceEdit(
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

		TextWorkspaceEdit? workspaceEdit = LuaLanguageServerResponseParser.ParseWorkspaceEdit(response);

		Assert.IsNull(workspaceEdit);
	}

	[TestMethod]
	public void ParseWorkspaceEdit_LogsWarningWhenDocumentChangesContainUnsupportedResourceOperation()
	{
		string firstPath = Path.GetFullPath(@"C:\Workspace\Scripts\first.lua");
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\second.lua");

		WorkspaceEditResponse? response = DeserializeWorkspaceEditResponse(new
		{
			documentChanges = new object[]
			{
				new
				{
					kind = "rename",
					oldUri = new Uri(firstPath).AbsoluteUri,
					newUri = new Uri(secondPath).AbsoluteUri
				}
			}
		});

		using var logScope = new NLogMemoryScope(LogLevel.Warn);

		TextWorkspaceEdit? workspaceEdit = LuaLanguageServerResponseParser.ParseWorkspaceEdit(response);

		Assert.IsNull(workspaceEdit);
		Assert.AreEqual(1, logScope.Logs.Count);
		StringAssert.Contains(logScope.Logs[0], "unsupported resource operation");
		StringAssert.Contains(logScope.Logs[0], "rename workspace edit");
		StringAssert.Contains(logScope.Logs[0], "first.lua");
		StringAssert.Contains(logScope.Logs[0], "second.lua");
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
		IReadOnlyList<TextEdit> textEdits = LuaLanguageServerResponseParser.ParseDocumentFormattingEdits(
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
}
