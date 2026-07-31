using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the typed top-level semantic token response for both full and delta results.
/// </summary>
/// <param name="ResultId">The server-provided result identifier for future delta requests.</param>
/// <param name="Data">The raw semantic token integer stream.</param>
/// <param name="Edits">The semantic token edits for delta responses.</param>
public readonly record struct SemanticTokensWireResponse(
	[property: JsonPropertyName("resultId")] string? ResultId,
	[property: JsonPropertyName("data")] int[]? Data,
	[property: JsonPropertyName("edits")] SemanticTokensEditPayload[]? Edits);

/// <summary>
/// Represents a semantic token edit payload as returned on the wire by the language server.
/// </summary>
/// <param name="Start">The start index of the edit in the token stream.</param>
/// <param name="DeleteCount">The number of tokens to delete at the start index.</param>
/// <param name="Data">The replacement token data to insert.</param>
public readonly record struct SemanticTokensEditPayload(
	[property: JsonPropertyName("start")] int? Start,
	[property: JsonPropertyName("deleteCount")] int? DeleteCount,
	[property: JsonPropertyName("data")] int[]? Data);
