using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a structured document-change entry within a workspace edit response.
/// </summary>
public readonly record struct WorkspaceDocumentChangePayload
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceDocumentChangePayload"/> struct.
	/// </summary>
	/// <param name="textDocument">The target text document descriptor.</param>
	/// <param name="edits">The edits to apply to the target document.</param>
	/// <param name="kind">The resource-operation kind when the change is not a text-document edit.</param>
	/// <param name="uri">The target URI for create or delete operations.</param>
	/// <param name="oldUri">The source URI for rename operations.</param>
	/// <param name="newUri">The destination URI for rename operations.</param>
	[JsonConstructor]
	public WorkspaceDocumentChangePayload(
		TextDocumentUriPayload? textDocument,
		IReadOnlyList<TextEditPayload>? edits,
		string? kind,
		string? uri,
		string? oldUri,
		string? newUri)
	{
		TextDocument = textDocument;
		Edits = WorkspaceEditPayloadCloner.CloneEditList(edits);
		Kind = kind;
		Uri = uri;
		OldUri = oldUri;
		NewUri = newUri;
	}

	/// <summary>
	/// Gets the target text document descriptor.
	/// </summary>
	[JsonPropertyName("textDocument")]
	public TextDocumentUriPayload? TextDocument { get; }

	/// <summary>
	/// Gets the edits to apply to the target document.
	/// The returned list is a defensive read-only snapshot.
	/// </summary>
	[JsonPropertyName("edits")]
	public IReadOnlyList<TextEditPayload>? Edits { get; }

	/// <summary>
	/// Gets the resource-operation kind when the change is not a text-document edit.
	/// </summary>
	[JsonPropertyName("kind")]
	public string? Kind { get; }

	/// <summary>
	/// Gets the target URI for create or delete operations.
	/// </summary>
	[JsonPropertyName("uri")]
	public string? Uri { get; }

	/// <summary>
	/// Gets the source URI for rename operations.
	/// </summary>
	[JsonPropertyName("oldUri")]
	public string? OldUri { get; }

	/// <summary>
	/// Gets the destination URI for rename operations.
	/// </summary>
	[JsonPropertyName("newUri")]
	public string? NewUri { get; }

	/// <summary>
	/// Gets a value indicating whether the payload describes a resource operation rather than text edits.
	/// </summary>
	public bool IsResourceOperation => !string.IsNullOrWhiteSpace(Kind);
}
