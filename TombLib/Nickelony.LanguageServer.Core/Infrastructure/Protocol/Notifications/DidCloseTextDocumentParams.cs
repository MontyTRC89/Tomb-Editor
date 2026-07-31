using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the typed payload for a <c>textDocument/didClose</c> notification.
/// </summary>
/// <param name="TextDocument">The closed document identifier.</param>
public readonly record struct DidCloseTextDocumentParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument);
