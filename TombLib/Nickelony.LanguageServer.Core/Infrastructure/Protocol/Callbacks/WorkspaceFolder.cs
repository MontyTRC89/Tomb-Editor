using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a workspace folder advertised to the language server.
/// </summary>
/// <param name="Uri">The workspace folder URI.</param>
/// <param name="Name">The display name of the workspace folder.</param>
public readonly record struct WorkspaceFolder(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("name")] string Name);
