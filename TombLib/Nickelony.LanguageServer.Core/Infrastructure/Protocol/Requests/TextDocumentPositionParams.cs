using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a text-document request payload that targets a specific position.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Position">The targeted position within the document.</param>
public readonly record struct TextDocumentPositionParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] ProtocolPosition Position);
