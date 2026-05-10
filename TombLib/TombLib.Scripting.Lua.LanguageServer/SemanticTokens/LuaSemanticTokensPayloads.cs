using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the typed top-level semantic-token response for both full and delta results.
/// </summary>
public readonly record struct LuaSemanticTokensWireResponse(
	[property: JsonPropertyName("resultId")] string? ResultId,
	[property: JsonPropertyName("data")] int[]? Data,
	[property: JsonPropertyName("edits")] LuaSemanticTokensEditPayload[]? Edits);

public readonly record struct LuaSemanticTokensEditPayload(
	[property: JsonPropertyName("start")] int? Start,
	[property: JsonPropertyName("deleteCount")] int? DeleteCount,
	[property: JsonPropertyName("data")] int[]? Data);
