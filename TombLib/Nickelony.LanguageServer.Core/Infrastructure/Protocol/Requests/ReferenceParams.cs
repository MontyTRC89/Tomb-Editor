using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

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
