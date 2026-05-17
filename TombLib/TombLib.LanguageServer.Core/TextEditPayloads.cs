using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents a single text edit returned by the language server.
/// </summary>
/// <param name="Range">The replaced document range.</param>
/// <param name="NewText">The replacement text.</param>
public readonly record struct TextEditPayload(
	[property: JsonPropertyName("range")] ProtocolRangePayload? Range,
	[property: JsonPropertyName("newText")] string? NewText);

/// <summary>
/// Represents the typed top-level workspace edit response used by rename.
/// </summary>
/// <param name="Changes">The simple URI-to-edit map returned by the server.</param>
/// <param name="DocumentChanges">The structured document-change payload returned by the server.</param>
public readonly record struct WorkspaceEditResponse
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceEditResponse"/> struct.
	/// </summary>
	/// <param name="changes">The simple URI-to-edit map returned by the server.</param>
	/// <param name="documentChanges">The structured document-change payload returned by the server.</param>
	[JsonConstructor]
	public WorkspaceEditResponse(
		IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? changes,
		IReadOnlyList<WorkspaceDocumentChangePayload>? documentChanges)
	{
		Changes = WorkspaceEditPayloadCloner.CloneChangeMap(changes);
		DocumentChanges = WorkspaceEditPayloadCloner.CloneDocumentChanges(documentChanges);
	}

	/// <summary>
	/// Gets the simple URI-to-edit map returned by the server.
	/// The returned dictionary and nested edit lists are defensive read-only snapshots.
	/// </summary>
	[JsonPropertyName("changes")]
	public IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? Changes { get; }

	/// <summary>
	/// Gets the structured document-change payload returned by the server.
	/// The returned list is a defensive read-only snapshot.
	/// </summary>
	[JsonPropertyName("documentChanges")]
	public IReadOnlyList<WorkspaceDocumentChangePayload>? DocumentChanges { get; }
}

/// <summary>
/// Represents a structured document-change entry within a workspace edit response.
/// </summary>
/// <param name="TextDocument">The target text document descriptor.</param>
/// <param name="Edits">The edits to apply to the target document.</param>
/// <param name="Kind">The resource-operation kind when the change is not a text-document edit.</param>
/// <param name="Uri">The target URI for create or delete operations.</param>
/// <param name="OldUri">The source URI for rename operations.</param>
/// <param name="NewUri">The destination URI for rename operations.</param>
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

internal static class WorkspaceEditPayloadCloner
{
	public static IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? CloneChangeMap(
		IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? changes)
	{
		if (changes is null)
			return null;

		var clonedChanges = new Dictionary<string, IReadOnlyList<TextEditPayload>?>(changes.Count, StringComparer.Ordinal);

		foreach ((string uri, IReadOnlyList<TextEditPayload>? edits) in changes)
			clonedChanges[uri] = CloneEditList(edits);

		return new ReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>(clonedChanges);
	}

	public static IReadOnlyList<WorkspaceDocumentChangePayload>? CloneDocumentChanges(IReadOnlyList<WorkspaceDocumentChangePayload>? documentChanges)
		=> documentChanges is null ? null : Array.AsReadOnly([.. documentChanges]);

	public static IReadOnlyList<TextEditPayload>? CloneEditList(IReadOnlyList<TextEditPayload>? edits)
		=> edits is null ? null : Array.AsReadOnly([.. edits]);
}

/// <summary>
/// Identifies a text document by URI in structured workspace edits.
/// </summary>
/// <param name="Uri">The target document URI.</param>
public readonly record struct TextDocumentUriPayload(
	[property: JsonPropertyName("uri")] string? Uri);
