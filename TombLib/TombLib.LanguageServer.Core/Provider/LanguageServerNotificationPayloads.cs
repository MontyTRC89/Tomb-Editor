using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents the typed payload for a textDocument/didOpen notification.
/// </summary>
/// <param name="TextDocument">The opened document payload.</param>
public readonly record struct DidOpenTextDocumentParams(
	[property: JsonPropertyName("textDocument")] DidOpenTextDocumentPayload TextDocument);

/// <summary>
/// Describes the document opened by a <c>textDocument/didOpen</c> notification.
/// </summary>
/// <param name="Uri">The document URI.</param>
/// <param name="LanguageId">The language identifier understood by the server.</param>
/// <param name="Version">The initial document version.</param>
/// <param name="Text">The full document text.</param>
public readonly record struct DidOpenTextDocumentPayload(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("languageId")] string LanguageId,
	[property: JsonPropertyName("version")] int Version,
	[property: JsonPropertyName("text")] string Text);

/// <summary>
/// Identifies a versioned text document in protocol notifications.
/// </summary>
/// <param name="Uri">The document URI.</param>
/// <param name="Version">The current document version.</param>
public readonly record struct VersionedTextDocumentIdentifierPayload(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("version")] int Version);

/// <summary>
/// Represents the typed payload for a <c>textDocument/didChange</c> notification.
/// </summary>
/// <param name="TextDocument">The changed versioned document identifier.</param>
/// <param name="ContentChanges">The content changes to apply.</param>
public readonly record struct DidChangeTextDocumentParams(
	[property: JsonPropertyName("textDocument")] VersionedTextDocumentIdentifierPayload TextDocument,
	[property: JsonPropertyName("contentChanges")] TextDocumentContentChangePayload[] ContentChanges);

/// <summary>
/// Represents a single text content change within a change notification.
/// </summary>
/// <param name="Text">The replacement text.</param>
/// <param name="Range">The replaced range, or <see langword="null"/> for full-document replacement.</param>
public readonly record struct TextDocumentContentChangePayload(
	[property: JsonPropertyName("text")] string Text,
	[property: JsonPropertyName("range")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	ProtocolRangePayload? Range = null);

/// <summary>
/// Represents the typed payload for a <c>textDocument/didClose</c> notification.
/// </summary>
/// <param name="TextDocument">The closed document identifier.</param>
public readonly record struct DidCloseTextDocumentParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument);

/// <summary>
/// Represents the typed payload for a <c>workspace/didChangeConfiguration</c> notification.
/// </summary>
/// <param name="Settings">The current workspace settings object.</param>
public readonly record struct DidChangeConfigurationParams(
	[property: JsonPropertyName("settings")] object Settings);

/// <summary>
/// Represents the typed payload for a <c>workspace/didChangeWatchedFiles</c> notification.
/// </summary>
/// <param name="Changes">The watched file changes to report.</param>
public readonly record struct DidChangeWatchedFilesParams(
	[property: JsonPropertyName("changes")] FileEventPayload[] Changes);

/// <summary>
/// Represents a single watched file event.
/// </summary>
/// <param name="Uri">The affected file URI.</param>
/// <param name="Type">The protocol change kind.</param>
public readonly record struct FileEventPayload(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("type")] int Type);
