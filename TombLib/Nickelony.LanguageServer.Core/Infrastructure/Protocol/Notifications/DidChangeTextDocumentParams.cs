using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

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
