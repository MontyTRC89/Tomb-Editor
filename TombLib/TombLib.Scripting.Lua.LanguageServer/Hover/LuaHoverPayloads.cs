using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the typed top-level hover payload returned by LuaLS.
/// </summary>
public sealed record LuaHoverResponse
{
	[JsonPropertyName("contents")]
	public JsonElement Contents { get; init; }
}
