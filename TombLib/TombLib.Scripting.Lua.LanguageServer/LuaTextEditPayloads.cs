using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents a single text edit returned by LuaLS.
/// </summary>
public readonly record struct LuaTextEditPayload(
	[property: JsonPropertyName("range")] LuaProtocolRangePayload? Range,
	[property: JsonPropertyName("newText")] string? NewText);

/// <summary>
/// Represents the typed top-level workspace edit response used by rename.
/// </summary>
public readonly record struct LuaWorkspaceEditResponse(
	[property: JsonPropertyName("changes")] Dictionary<string, LuaTextEditPayload[]?>? Changes,
	[property: JsonPropertyName("documentChanges")] LuaWorkspaceDocumentChangePayload[]? DocumentChanges);

public readonly record struct LuaWorkspaceDocumentChangePayload(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentUriPayload? TextDocument,
	[property: JsonPropertyName("edits")] LuaTextEditPayload[]? Edits);

public readonly record struct LuaTextDocumentUriPayload(
	[property: JsonPropertyName("uri")] string? Uri);
