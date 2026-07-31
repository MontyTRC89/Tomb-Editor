using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the supported completion text-edit shapes returned by a language server.
/// </summary>
public sealed record CompletionTextEditPayload
{
	/// <summary>
	/// Gets the text inserted or replaced by the completion.
	/// </summary>
	[JsonPropertyName("newText")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? NewText { get; init; }

	/// <summary>
	/// Gets the single replace range used by classic completion text edits.
	/// </summary>
	[JsonPropertyName("range")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ProtocolRangePayload? Range { get; init; }

	/// <summary>
	/// Gets the insert range used by insert/replace completion edits.
	/// </summary>
	[JsonPropertyName("insert")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ProtocolRangePayload? Insert { get; init; }

	/// <summary>
	/// Gets the replace range used by insert/replace completion edits.
	/// </summary>
	[JsonPropertyName("replace")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ProtocolRangePayload? Replace { get; init; }
}
