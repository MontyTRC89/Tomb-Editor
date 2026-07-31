using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes the completion trigger context for a completion request.
/// </summary>
/// <param name="TriggerKind">The protocol trigger kind.</param>
/// <param name="TriggerCharacter">The trigger character when completion was character-triggered.</param>
public readonly record struct CompletionContextPayload(
	[property: JsonPropertyName("triggerKind")] int TriggerKind,
	[property: JsonPropertyName("triggerCharacter")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	string? TriggerCharacter = null);

/// <summary>
/// Represents a completion request payload.
/// </summary>
/// <param name="TextDocument">The targeted document.</param>
/// <param name="Position">The caret position for the completion request.</param>
/// <param name="Context">The completion trigger context.</param>
public readonly record struct CompletionParams(
	[property: JsonPropertyName("textDocument")] TextDocumentIdentifier TextDocument,
	[property: JsonPropertyName("position")] ProtocolPosition Position,
	[property: JsonPropertyName("context")] CompletionContextPayload Context);
