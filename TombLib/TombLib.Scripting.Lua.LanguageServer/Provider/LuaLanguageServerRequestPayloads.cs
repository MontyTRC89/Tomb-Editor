using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

public readonly record struct LuaTextDocumentIdentifier(
	[property: JsonPropertyName("uri")] string Uri);

public readonly record struct LuaProtocolPosition(
	[property: JsonPropertyName("line")] int Line,
	[property: JsonPropertyName("character")] int Character);

/// <summary>
/// Represents a text-document request payload that targets a specific position.
/// </summary>
public readonly record struct LuaTextDocumentPositionParams(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] LuaProtocolPosition Position);

public readonly record struct LuaCompletionContextPayload(
	[property: JsonPropertyName("triggerKind")] int TriggerKind,
	[property: JsonPropertyName("triggerCharacter")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? TriggerCharacter = null);

public readonly record struct LuaCompletionParams(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] LuaProtocolPosition Position,
	[property: JsonPropertyName("context")] LuaCompletionContextPayload Context);

public readonly record struct LuaReferenceContextPayload(
	[property: JsonPropertyName("includeDeclaration")] bool IncludeDeclaration);

public readonly record struct LuaReferenceParams(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] LuaProtocolPosition Position,
	[property: JsonPropertyName("context")] LuaReferenceContextPayload Context);

public readonly record struct LuaRenameParams(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] LuaProtocolPosition Position,
	[property: JsonPropertyName("newName")] string NewName);

public readonly record struct LuaFormattingOptionsPayload(
	[property: JsonPropertyName("tabSize")] int TabSize,
	[property: JsonPropertyName("insertSpaces")] bool InsertSpaces);

public readonly record struct LuaDocumentFormattingParams(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("options")] LuaFormattingOptionsPayload Options);

public readonly record struct LuaSemanticTokensParams(
	[property: JsonPropertyName("textDocument")] LuaTextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("previousResultId")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? PreviousResultId = null);
