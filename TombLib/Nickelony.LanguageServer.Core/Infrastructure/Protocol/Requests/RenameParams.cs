using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a rename request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Position">The symbol position to rename.</param>
/// <param name="NewName">The replacement symbol name.</param>
public readonly record struct RenameParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] ProtocolPosition Position,
	[property: JsonPropertyName("newName")] string NewName);
