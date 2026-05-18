using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents a zero-based protocol position within a text document.
/// </summary>
/// <param name="Line">The zero-based line index.</param>
/// <param name="Character">The zero-based character index on the line.</param>
public readonly record struct ProtocolPosition(
	[property: JsonPropertyName("line")] int Line,
	[property: JsonPropertyName("character")] int Character);
