using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the typed payload for a textDocument/didOpen notification.
/// </summary>
public readonly record struct LuaDidOpenTextDocumentParams(
	[property: JsonPropertyName("textDocument")] LuaDidOpenTextDocumentPayload TextDocument);

public readonly record struct LuaDidOpenTextDocumentPayload(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("languageId")] string LanguageId,
	[property: JsonPropertyName("version")] int Version,
	[property: JsonPropertyName("text")] string Text);

public readonly record struct LuaVersionedTextDocumentIdentifierPayload(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("version")] int Version);

public readonly record struct LuaDidChangeTextDocumentParams(
	[property: JsonPropertyName("textDocument")] LuaVersionedTextDocumentIdentifierPayload TextDocument,
	[property: JsonPropertyName("contentChanges")] LuaTextDocumentContentChangePayload[] ContentChanges);

public readonly record struct LuaTextDocumentContentChangePayload(
	[property: JsonPropertyName("text")] string Text,
	[property: JsonPropertyName("range")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	LuaProtocolRangePayload? Range = null);

public readonly record struct LuaDidCloseTextDocumentParams(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentIdentifier TextDocument);

public readonly record struct LuaDidChangeConfigurationParams(
	[property: JsonPropertyName("settings")] object Settings);

public readonly record struct LuaDidChangeWatchedFilesParams(
	[property: JsonPropertyName("changes")] LuaFileEventPayload[] Changes);

public readonly record struct LuaFileEventPayload(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("type")] int Type);
