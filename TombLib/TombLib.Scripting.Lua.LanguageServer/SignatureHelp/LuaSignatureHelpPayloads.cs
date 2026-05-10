using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the typed top-level signature-help payload returned by LuaLS.
/// </summary>
public sealed record LuaSignatureHelpResponse
{
	[JsonPropertyName("activeSignature")]
	public int? ActiveSignature { get; init; }

	[JsonPropertyName("activeParameter")]
	public int? ActiveParameter { get; init; }

	[JsonPropertyName("signatures")]
	public LuaSignatureHelpSignaturePayload[]? Signatures { get; init; }
}

public sealed record LuaSignatureHelpSignaturePayload
{
	[JsonPropertyName("label")]
	public string? Label { get; init; }

	[JsonPropertyName("documentation")]
	public JsonElement Documentation { get; init; }

	[JsonPropertyName("activeParameter")]
	public int? ActiveParameter { get; init; }

	[JsonPropertyName("parameters")]
	public LuaSignatureHelpParameterPayload[]? Parameters { get; init; }
}

public sealed record LuaSignatureHelpParameterPayload
{
	[JsonPropertyName("label")]
	public JsonElement Label { get; init; }

	[JsonPropertyName("documentation")]
	public JsonElement Documentation { get; init; }
}
