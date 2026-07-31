using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a workspace configuration request payload.
/// </summary>
/// <param name="Items">The requested configuration sections.</param>
public readonly record struct WorkspaceConfigurationParams(
	[property: JsonPropertyName("items")] WorkspaceConfigurationItem[]? Items);

/// <summary>
/// Identifies a single configuration section requested from the host.
/// </summary>
/// <param name="Section">The dotted configuration section name.</param>
public readonly record struct WorkspaceConfigurationItem(
	[property: JsonPropertyName("section")] string? Section);
