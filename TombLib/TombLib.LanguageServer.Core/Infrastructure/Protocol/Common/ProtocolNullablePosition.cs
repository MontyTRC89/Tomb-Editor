using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents a nullable zero-based protocol position.
/// </summary>
/// <param name="Line">The zero-based line index.</param>
/// <param name="Character">The zero-based character index.</param>
public readonly record struct ProtocolNullablePosition(
	[property: JsonPropertyName("line")] int? Line,
	[property: JsonPropertyName("character")] int? Character);
