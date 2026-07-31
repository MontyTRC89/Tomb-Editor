using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a single typed completion item returned by a language server.
/// </summary>
public sealed record CompletionItemPayload
{
	/// <summary>
	/// Gets the display label shown for the completion item.
	/// </summary>
	[JsonPropertyName("label")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Label { get; init; }

	/// <summary>
	/// Gets the protocol completion-item kind.
	/// </summary>
	[JsonPropertyName("kind")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Kind { get; init; }

	/// <summary>
	/// Gets the optional detail text shown beside the label.
	/// </summary>
	[JsonPropertyName("detail")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Detail { get; init; }

	/// <summary>
	/// Gets the optional documentation payload for the completion item.
	/// </summary>
	[JsonPropertyName("documentation")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public JsonElement? Documentation { get; init; }

	/// <summary>
	/// Gets the explicit insert text when it differs from the label.
	/// </summary>
	[JsonPropertyName("insertText")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? InsertText { get; init; }

	/// <summary>
	/// Gets the protocol insert-text format.
	/// </summary>
	[JsonPropertyName("insertTextFormat")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? InsertTextFormat { get; init; }

	/// <summary>
	/// Gets the optional filter text used for completion matching.
	/// </summary>
	[JsonPropertyName("filterText")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? FilterText { get; init; }

	/// <summary>
	/// Gets a value indicating whether the item should be preselected by the client.
	/// </summary>
	[JsonPropertyName("preselect")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Preselect { get; init; }

	/// <summary>
	/// Gets the text edit applied when the completion item is committed.
	/// </summary>
	[JsonPropertyName("textEdit")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public CompletionTextEditPayload? TextEdit { get; init; }

	/// <summary>
	/// Gets protocol fields not modeled explicitly by this lean wrapper.
	/// These values round-trip so completion-item resolve can preserve opaque server state such as <c>data</c>.
	/// </summary>
	[JsonExtensionData]
	public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
