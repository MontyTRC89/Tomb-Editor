using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents the typed top-level semantic-token response for both full and delta results.
/// </summary>
public readonly record struct SemanticTokensWireResponse(
	[property: JsonPropertyName("resultId")] string? ResultId,
	[property: JsonPropertyName("data")] int[]? Data,
	[property: JsonPropertyName("edits")] SemanticTokensEditPayload[]? Edits);

/// <summary>
/// Represents a semantic-token edit payload as returned on the wire by the language server.
/// </summary>
public readonly record struct SemanticTokensEditPayload(
	[property: JsonPropertyName("start")] int? Start,
	[property: JsonPropertyName("deleteCount")] int? DeleteCount,
	[property: JsonPropertyName("data")] int[]? Data);
