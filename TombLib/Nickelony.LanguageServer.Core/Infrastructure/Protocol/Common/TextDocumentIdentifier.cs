using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Identifies a single text document in protocol payloads.
/// </summary>
/// <param name="Uri">The document URI.</param>
public readonly record struct TextDocumentIdentifier(
	[property: JsonPropertyName("uri")] string Uri);
