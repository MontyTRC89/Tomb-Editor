using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Identifies a single text document in protocol request payloads.
/// </summary>
/// <param name="Uri">The document URI.</param>
public readonly record struct TextDocumentIdentifier(
	[property: JsonPropertyName("uri")] string Uri);

/// <summary>
/// Represents a zero-based protocol position within a text document.
/// </summary>
/// <param name="Line">The zero-based line index.</param>
/// <param name="Character">The zero-based character index on the line.</param>
public readonly record struct ProtocolPosition(
	[property: JsonPropertyName("line")] int Line,
	[property: JsonPropertyName("character")] int Character);

/// <summary>
/// Represents a text-document request payload that targets a specific position.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Position">The targeted position within the document.</param>
public readonly record struct TextDocumentPositionParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] ProtocolPosition Position);

/// <summary>
/// Describes the completion trigger context for a completion request.
/// </summary>
/// <param name="TriggerKind">The protocol trigger kind.</param>
/// <param name="TriggerCharacter">The trigger character when completion was character-triggered.</param>
public readonly record struct CompletionContextPayload(
	[property: JsonPropertyName("triggerKind")] int TriggerKind,
	[property: JsonPropertyName("triggerCharacter")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? TriggerCharacter = null);

/// <summary>
/// Represents a completion request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Position">The caret position for the completion request.</param>
/// <param name="Context">The completion trigger context.</param>
public readonly record struct CompletionParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] ProtocolPosition Position,
	[property: JsonPropertyName("context")] CompletionContextPayload Context);

/// <summary>
/// Describes how reference requests should treat declarations.
/// </summary>
/// <param name="IncludeDeclaration">Whether declaration locations should be included in the response.</param>
public readonly record struct ReferenceContextPayload(
	[property: JsonPropertyName("includeDeclaration")] bool IncludeDeclaration);

/// <summary>
/// Represents a references request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Position">The symbol position to query.</param>
/// <param name="Context">The request context controlling declaration inclusion.</param>
public readonly record struct ReferenceParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] ProtocolPosition Position,
	[property: JsonPropertyName("context")] ReferenceContextPayload Context);

/// <summary>
/// Represents a rename request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Position">The symbol position to rename.</param>
/// <param name="NewName">The replacement symbol name.</param>
public readonly record struct RenameParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] ProtocolPosition Position,
	[property: JsonPropertyName("newName")] string NewName);

/// <summary>
/// Represents editor formatting options sent with a document-formatting request.
/// </summary>
/// <param name="TabSize">The indentation size in columns.</param>
/// <param name="InsertSpaces">Whether indentation should use spaces instead of tabs.</param>
public readonly record struct FormattingOptionsPayload(
	[property: JsonPropertyName("tabSize")] int TabSize,
	[property: JsonPropertyName("insertSpaces")] bool InsertSpaces);

/// <summary>
/// Represents a document-formatting request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Options">The formatting options to apply.</param>
public readonly record struct DocumentFormattingParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("options")] FormattingOptionsPayload Options);

/// <summary>
/// Represents a semantic-tokens request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="PreviousResultId">The previously cached semantic-tokens result identifier, when requesting a delta.</param>
public readonly record struct SemanticTokensParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("previousResultId")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? PreviousResultId = null);
