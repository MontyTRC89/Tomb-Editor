using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.Scripting.Lua.LanguageServer;

public readonly record struct LuaEmptyParams();

public readonly record struct LuaWorkspaceConfigurationParams(
	[property: JsonPropertyName("items")] LuaWorkspaceConfigurationItem[]? Items);

public readonly record struct LuaWorkspaceConfigurationItem(
	[property: JsonPropertyName("section")] string? Section);

public readonly record struct LuaWorkspaceFolder(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("name")] string Name);

public readonly record struct LuaWindowMessageParams(
	[property: JsonPropertyName("type")] int? Type,
	[property: JsonPropertyName("message")] string? Message);

/// <summary>
/// Represents a typed diagnostics notification raised by LuaLS for a tracked document.
/// </summary>
public readonly record struct LuaPublishDiagnosticsParams(
	[property: JsonPropertyName("uri")] string? Uri,
	[property: JsonPropertyName("version")] int? Version,
	[property: JsonPropertyName("diagnostics")] LuaDiagnosticPayload[]? Diagnostics);

/// <summary>
/// Represents a single diagnostic entry from a publish-diagnostics notification.
/// </summary>
public readonly record struct LuaDiagnosticPayload(
	[property: JsonPropertyName("range")] LuaProtocolRangePayload? Range,
	[property: JsonPropertyName("severity")] int? Severity,
	[property: JsonPropertyName("message")] string? Message,
	[property: JsonPropertyName("source")] string? Source,
	[property: JsonPropertyName("code")] JsonElement? Code);

public readonly record struct LuaProtocolRangePayload(
	[property: JsonPropertyName("start")] LuaProtocolNullablePosition? Start,
	[property: JsonPropertyName("end")] LuaProtocolNullablePosition? End);

public readonly record struct LuaProtocolNullablePosition(
	[property: JsonPropertyName("line")] int? Line,
	[property: JsonPropertyName("character")] int? Character);
