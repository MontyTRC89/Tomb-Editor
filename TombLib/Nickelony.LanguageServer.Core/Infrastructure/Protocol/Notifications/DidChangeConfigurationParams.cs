using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the typed payload for a <c>workspace/didChangeConfiguration</c> notification.
/// </summary>
/// <param name="Settings">The current workspace settings object.</param>
public readonly record struct DidChangeConfigurationParams(
	[property: JsonPropertyName("settings")] object Settings);
