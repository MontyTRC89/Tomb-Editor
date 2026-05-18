using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents a message notification sent from the language server.
/// </summary>
/// <param name="Type">The protocol message severity.</param>
/// <param name="Message">The message text.</param>
public readonly record struct WindowMessageParams(
	[property: JsonPropertyName("type")] int? Type,
	[property: JsonPropertyName("message")] string? Message);
