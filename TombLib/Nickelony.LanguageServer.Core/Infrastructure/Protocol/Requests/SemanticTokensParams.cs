using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a semantic tokens request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="PreviousResultId">The previously cached semantic tokens result identifier, when requesting a delta.</param>
public readonly record struct SemanticTokensParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("previousResultId")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? PreviousResultId = null);
