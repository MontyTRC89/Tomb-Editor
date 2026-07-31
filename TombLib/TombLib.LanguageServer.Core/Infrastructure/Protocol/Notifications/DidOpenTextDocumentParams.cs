using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents the typed payload for a <c>textDocument/didOpen</c> notification.
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
