using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents a single typed reference location returned by LuaLS.
/// </summary>
public sealed record LuaReferenceResponse
{
	[JsonPropertyName("uri")]
	public string? Uri { get; init; }

	[JsonPropertyName("range")]
	public LuaProtocolRangePayload? Range { get; init; }
}
