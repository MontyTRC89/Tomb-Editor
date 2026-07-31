using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Identifies a text document by URI in structured workspace edits.
/// </summary>
/// <param name="Uri">The target document URI.</param>
public readonly record struct TextDocumentUriPayload(
	[property: JsonPropertyName("uri")] string? Uri);
